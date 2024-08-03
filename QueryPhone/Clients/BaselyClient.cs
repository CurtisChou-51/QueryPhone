using Microsoft.Extensions.Logging;
using QueryPhone.Model;
using System.Text.RegularExpressions;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace QueryPhone.Clients
{
	public class BaselyClient : IQueryPhoneClient
	{
		private readonly ILogger _logger;
		private readonly HttpClient client = new HttpClient();

		private string QueryUrl(string phone) => $"https://base.ly/tw/{phone}";

		public BaselyClient(ILogger<TellowsClient> logger)
		{
			_logger = logger;
		}

		public string GetName()
		{
			return "Basely 搜索電話號碼";
		}

		public async Task<QueryPhoneResult> QueryAsync(string phone)
		{
			try 
			{
				HtmlDocument doc = await QueryToDocImpl(phone);

				var reportMsgs = ExtReports(doc).Distinct().ToList();
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
			var nodes = doc.DocumentNode.SelectNodes("//div[@class='comments']//article");
			if (nodes == null)
				yield break;

			foreach (var node in nodes)
				yield return Regex.Replace(node.InnerText, @"\s+", " ").Trim();
		}

		private IEnumerable<string> ExtSummary(HtmlDocument doc)
		{
			var node = doc.DocumentNode.SelectSingleNode("//div[@class='phone-summary']");
			if (node == null)
				yield break;

			yield return Regex.Replace(node.InnerText, @"\s+", " ").Trim();
		}

		private async Task<HtmlDocument> QueryToDocImpl(string phone)
		{
			client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36");
			client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
			var resp = await client.GetStringAsync(QueryUrl(phone));
			HtmlDocument doc = new HtmlDocument();
			doc.LoadHtml(resp);
			return doc;
		}

	}
}
