using Microsoft.Extensions.Logging;
using QueryPhone.Model;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace QueryPhone.Clients
{
    public class WhosNumberClient : IQueryPhoneClient
	{
		private readonly ILogger _logger;
		private readonly HttpClient client = new HttpClient();
		private string QueryUrl(string phone) => $"https://whosnumber.com/tw/{phone}";

		public WhosNumberClient(ILogger<WhosNumberClient> logger)
		{
			_logger = logger;
		}

		public string GetName()
		{
			return "WhosNumber 到底是誰打來的電話";
		}

		public async Task<QueryPhoneResult> QueryAsync(string phone)
		{
			try
			{
				HtmlDocument doc = await QueryToDocImpl(phone);

				var summaryMsgs = ExtSummary(doc).Distinct().ToList();
				var reportMsgs = ExtReports(doc).Distinct().ToList();
				reportMsgs = reportMsgs.GroupBy(x => x).Select(g => $"{g.Key} ({g.Count()})").ToList();

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

		private IEnumerable<string> ExtSummary(HtmlDocument doc)
		{
			var infoNode = doc.DocumentNode.SelectSingleNode("//div[@class='col-md-4']//p[contains(., '這個號碼的基本信息')]");
			if (infoNode == null)
				yield break;

			var elem = infoNode;
			while (elem.Name != "div")
				elem = elem.NextSibling;

			string? text = elem?.InnerText?.Trim();
			if (!string.IsNullOrWhiteSpace(text))
				yield return text;
		}

		private IEnumerable<string> ExtReports(HtmlDocument doc)
		{
			var imgNodes = doc.DocumentNode.SelectNodes("//div[@class='panel-body']//img");
			if (imgNodes == null)
				yield break;
			foreach (var imgNode in imgNodes)
			{
				string? text = imgNode.NextSibling.InnerText?.Trim();
				if (!string.IsNullOrWhiteSpace(text))
					yield return text;
			}
		}

		private async Task<HtmlDocument> QueryToDocImpl(string phone)
		{
			var resp = await client.GetStringAsync(QueryUrl(phone));
			HtmlDocument doc = new HtmlDocument();
			doc.LoadHtml(resp);
			return doc;
		}
	}
}
