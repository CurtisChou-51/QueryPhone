using Microsoft.Extensions.Logging;
using QueryPhone.Core.Models;
using System.Text.RegularExpressions;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace QueryPhone.Core.Clients
{
    public class PhoneBookClient : IQueryPhoneClient
    {
        private readonly ILogger _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        private string QueryUrl(string phone) => $"https://phone-book.tw/search/{phone}.html";

        public PhoneBookClient(ILogger<PhoneBookClient> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public string Name => "PhoneBook 黃頁電話簿";

        public async Task<QueryPhoneResult> QueryAsync(string phone)
        {
            try
            {
                var resp = await QueryImpl(phone);
                if (resp.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    // 403 Forbidden, retry after 0.5 second
                    await Task.Delay(500);
                    resp = await QueryImpl(phone);
                }
                string respStr = await resp.Content.ReadAsStringAsync();
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(respStr);

                var reportMsgs = YieldReportMsgs(doc).Distinct().ToList();
                var summaryMsgs = YieldSummaryMsgs(doc).Distinct().ToList();

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

        /// <summary> 提取總評文字 </summary>
        private static IEnumerable<string> YieldSummaryMsgs(HtmlDocument doc)
        {
            var summaryAreaNode = doc.DocumentNode.SelectSingleNode("//div[@class='col-12' and contains(., '黃頁電話簿查詢') and contains(., '查詢次數')]");
            if (summaryAreaNode == null)
                yield break;

            string? summary = summaryAreaNode.SelectSingleNode("..//h1[contains(., '簡易摘要')]")?.InnerText?.Trim();
            if (!string.IsNullOrWhiteSpace(summary))
                yield return Regex.Replace(summary, @"\s+", string.Empty);

            string? vote = summaryAreaNode.SelectSingleNode("..//h1[contains(., '可信賴') and contains(., '不可信賴') and contains(., '票')]")?.InnerText?.Trim();
            if (!string.IsNullOrWhiteSpace(vote))
                yield return Regex.Replace(vote, @"\s+", string.Empty);

            string? queryTimes = summaryAreaNode.SelectSingleNode("..//h2[contains(., '查詢次數')]")?.InnerText?.Trim();
            if (!string.IsNullOrWhiteSpace(queryTimes))
                yield return queryTimes;
        }

        /// <summary> 提取用戶回報文字 </summary>
        private static IEnumerable<string> YieldReportMsgs(HtmlDocument doc)
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

        private Task<HttpResponseMessage> QueryImpl(string phone)
        {
            var client = _httpClientFactory.CreateClient();
            var req = new HttpRequestMessage(HttpMethod.Get, QueryUrl(phone));
            req.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36");
            return client.SendAsync(req);
        }
    }
}
