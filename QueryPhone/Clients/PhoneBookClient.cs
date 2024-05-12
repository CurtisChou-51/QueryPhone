using System.Net;
using System.Reflection;
using System.Text;
using QueryPhone.Model;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace QueryPhone.Clients
{
    public class PhoneBookClient : IQueryPhoneClient
	{
		private string QueryUrl(string phone) => $"https://phone-book.tw/search/{phone}.html";

		public string GetName()
		{
			return "PhoneBook 黃頁電話簿";
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
				return new QueryPhoneResult { QueryUrl = QueryUrl(phone) };
			}
		}

		private IEnumerable<string> ExtSummary(HtmlDocument doc)
		{
			var summaryAreaNode = doc.DocumentNode.SelectSingleNode("//div[@class='col-12' and contains(., '黃頁電話簿查詢') and contains(., '查詢次數')]");
			if (summaryAreaNode == null)
				yield break;

			string? summary = summaryAreaNode.SelectSingleNode("..//h1[contains(., '簡易摘要')]")?.InnerText?.Trim();
			if (!string.IsNullOrWhiteSpace(summary))
				yield return summary;

			string? vote = summaryAreaNode.SelectSingleNode("..//h1[contains(., '可信賴') and contains(., '不可信賴') and contains(., '票')]")?.InnerText?.Trim();
			if (!string.IsNullOrWhiteSpace(vote))
				yield return vote;

			string? queryTimes = summaryAreaNode.SelectSingleNode("..//h2[contains(., '查詢次數')]")?.InnerText?.Trim();
			if (!string.IsNullOrWhiteSpace(queryTimes))
				yield return queryTimes;
		}

		private IEnumerable<string> ExtReports(HtmlDocument doc)
		{
			var titleNodes = doc.DocumentNode.SelectNodes("//h1[@class='card-title']");
			if (titleNodes == null)
				yield break;
			foreach (var titleNode in titleNodes)
			{
				if (!string.IsNullOrWhiteSpace(titleNode.InnerText))
					yield return titleNode.InnerText.Trim();
			}
		}

		private async Task<string> QueryImpl(string phone)
		{
			var request = HttpWebRequest.Create(QueryUrl(phone));
			request.Method = "GET";

			// 此網站會檢查header，不通過會回應403
			// Host的順序必須在首位、User-Agent須為瀏覽器
			MethodInfo? priMethod = request.Headers.GetType().GetMethod("AddWithoutValidate", BindingFlags.Instance | BindingFlags.NonPublic);
			priMethod?.Invoke(request.Headers, new[] { "Host", "phone-book.tw" });
			priMethod?.Invoke(request.Headers, new[] { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36" });

			var response = await request.GetResponseAsync();
			var stream = response.GetResponseStream();
			using StreamReader reader = new StreamReader(stream, Encoding.UTF8);
			return await reader.ReadToEndAsync();
		}

		private async Task<HtmlDocument> QueryToDocImpl(string phone)
		{
			var resp = await QueryImpl(phone);
			HtmlDocument doc = new HtmlDocument();
			doc.LoadHtml(resp);
			return doc;
		}
	}
}
