using Microsoft.Extensions.Logging;
using QueryPhone.Core.Models;
using System.Text.RegularExpressions;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace QueryPhone.Core.Clients
{
    public class SlicklyClient : IQueryPhoneClient
    {
        private readonly ILogger _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        private string QueryUrl(string phone) => $"https://slick.ly/tw/{phone}";

        public SlicklyClient(ILogger<BaselyClient> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public string Name => "Slickly 搜索和分享未知電話號碼";

        public async Task<QueryPhoneResult> QueryAsync(string phone)
        {
            try
            {
                var resp = await QueryImpl(phone);
                if (resp.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return new QueryPhoneResult
                    {
                        Success = true,
                        QueryUrl = QueryUrl(phone),
                        ReportMsgs = ["查無資料"],
                        SummaryMsgs = ["查無資料"]
                    };
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

        /// <summary> 提取用戶回報文字 </summary>
        private static IEnumerable<string> YieldReportMsgs(HtmlDocument doc)
        {
            var nodes = doc.DocumentNode.SelectNodes("//div[@class='comments']//article");
            if (nodes == null)
                yield break;

            foreach (var node in nodes)
                yield return Regex.Replace(node.InnerText, @"\s+", " ").Trim();
        }

        /// <summary> 提取總評文字 </summary>
        private static IEnumerable<string> YieldSummaryMsgs(HtmlDocument doc)
        {
            var nodes = doc.DocumentNode.SelectNodes("//div[@class='summary-keywords']//div");
            if (nodes == null)
                yield break;

            foreach (var node in nodes)
                yield return Regex.Replace(node.InnerText, @"\s+", " ").Trim();
        }

        private Task<HttpResponseMessage> QueryImpl(string phone)
        {
            var client = _httpClientFactory.CreateClient();
            var req = new HttpRequestMessage(HttpMethod.Get, QueryUrl(phone));
            req.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36");
            req.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
            return client.SendAsync(req);
        }

    }
}
