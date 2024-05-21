using Microsoft.Extensions.Logging;
using QueryPhone.Model;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace QueryPhone.Clients
{
    public class WhocallClient : IQueryPhoneClient
	{
		private readonly ILogger _logger;
		private readonly HttpClient client = new HttpClient();

		private string QueryUrl(string phone) => $"https://whocall.cc/search/{phone}";

		public WhocallClient(ILogger<WhocallClient> logger)
		{
			_logger = logger;
		}

		public string GetName()
		{
			return "Whocall 查電話";
		}

		public async Task<QueryPhoneResult> QueryAsync(string phone)
		{
			try 
			{
				HtmlDocument doc = await QueryToDocImpl(phone);

				List<string> reportMsgs = ExtReports(doc).ToList();
				foreach (var page in ExtPageLinks(doc).Where(x => x != 1))
				{
					var docp = await QueryToDocImpl(phone, page);
					reportMsgs.AddRange(ExtReports(docp));
				}
				reportMsgs = reportMsgs.GroupBy(x => x).Select(g => $"{g.Key} ({g.Count()})").ToList();
				var summaryMsgs = ExtSummary(doc).Distinct().ToList();

				return new QueryPhoneResult
				{
					Success = true,
					QueryUrl = QueryUrl(phone),
					ReportMsgs = reportMsgs,
					SummaryMsgs = summaryMsgs
				};
			} 
			catch (Exception ex)
			{
				_logger.LogError(ex.ToString());
				return new QueryPhoneResult { QueryUrl = QueryUrl(phone) };
			}
		}

		private IEnumerable<string> ExtReports(HtmlDocument doc)
		{
			var row = doc.DocumentNode.SelectSingleNode("//dic[@class='row mt-2' and contains(., '最近回報')]");
			if (row == null)
				yield break;

			while (row.Name != "div")
				row = row.NextSibling;

			foreach (var node in row.SelectNodes(".//p[@class='card-text']"))
			{
				yield return node.InnerText;
			}
		}

		private IEnumerable<string> ExtSummary(HtmlDocument doc)
		{
			var cardBodyNode = doc.DocumentNode.SelectSingleNode("//dic[@class='card-body' and contains(., '電話資料分析')]");
			if (cardBodyNode == null)
				yield break;

			var queryTimeNode = cardBodyNode.SelectSingleNode("..//tr[contains(., '查詢次數')]");
			string? queryTime = queryTimeNode?.InnerText?.Trim();
			if (!string.IsNullOrWhiteSpace(queryTime))
				yield return queryTime;

			var titleNode = cardBodyNode.SelectSingleNode("..//div[@class='row text-center' and contains(., '信賴度分析:')]");
			string? info = titleNode?.NextSibling?.InnerText?.Trim()?.Replace("這個電話號碼尚未有足夠資料分析", "");
			if (!string.IsNullOrWhiteSpace(info))
				yield return info;
		}

		private IEnumerable<int> ExtPageLinks(HtmlDocument doc)
		{
			var links = doc.DocumentNode.SelectNodes("//a[@class='page-link']");
			if (links == null)
				yield break;

			foreach (var link in links)
			{ 
				string? linkText = link.InnerText?.Trim();
				if (linkText != null && int.TryParse(linkText, out int i))
					yield return i;
			}
		}

		private async Task<HtmlDocument> QueryToDocImpl(string phone, int page = 1)
		{
			string url = QueryUrl(phone);
			if (page > 1)
				url += $"?page={page}";
			var resp = await client.GetStringAsync(url);
			HtmlDocument doc = new HtmlDocument();
			doc.LoadHtml(resp);
			return doc;
		}

	}
}
