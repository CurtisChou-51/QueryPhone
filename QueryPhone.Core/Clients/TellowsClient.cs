using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using QueryPhone.Core.Models;
using System.Text.RegularExpressions;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace QueryPhone.Core.Clients
{
    public class TellowsClient : IQueryPhoneClient
    {
        private readonly ILogger _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        private string QueryUrl(string phone) => $"https://www.tellows.tw/num/{phone}";

        public TellowsClient(ILogger<TellowsClient> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public string Name => "Tellows 誰打來的電話";

        public async Task<QueryPhoneResult> QueryAsync(string phone)
        {
            try
            {
                var resp = await QueryImpl(phone);
                string respStr = await resp.Content.ReadAsStringAsync();
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(respStr);

                var reportMsgs = YieldReportMsgs(doc).Distinct().ToList();
                var summaryMsgs = YieldSummaryMsgs(doc).Distinct().ToList();

                // 將[總評]與[用戶回報]重複出現的部分自[總評]中移除
                summaryMsgs = summaryMsgs.Where(cm => !reportMsgs.Any(tm => tm.StartsWith(cm))).ToList();

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
            var nodes = doc.DocumentNode.SelectNodes("//table[@class='table-rating']//th");
            if (nodes == null)
                yield break;

            foreach (HtmlNode node in nodes)
            {
                string text = Regex.Replace(node.InnerText, @"\s+", " ").Trim();
                if (!string.IsNullOrWhiteSpace(text))
                    yield return text;
            }
        }

        /// <summary> 提取總評文字 </summary>
        private static IEnumerable<string> YieldSummaryMsgs(HtmlDocument doc)
        {
            var cardBody = doc.DocumentNode.SelectSingleNode("//div[@class='card-body']");
            if (cardBody == null)
                yield break;

            string? callType = cardBody.SelectSingleNode(".//b[contains(., '來電種類:')]")?.NextSibling.InnerText?.Trim();
            if (!string.IsNullOrWhiteSpace(callType))
                yield return callType;

            string? callerId = cardBody.SelectSingleNode(".//span[@class='callerId']")?.InnerText?.Trim();
            if (!string.IsNullOrWhiteSpace(callerId))
                yield return callerId;

            string? callerName = cardBody.SelectSingleNode(".//b[contains(., '來電者姓名:')]")?.NextSibling.InnerText?.Trim();
            if (!string.IsNullOrWhiteSpace(callerName))
                yield return callerName;

            string? statsqueries = cardBody.SelectSingleNode(".//strong[contains(., '搜尋次數:')]")?.InnerText?.Trim();
            if (!string.IsNullOrWhiteSpace(statsqueries))
                yield return Regex.Replace(statsqueries, @"\s+", " ");

            string? score = cardBody.SelectSingleNode(".//strong[text()='評分:']")?.NextSibling.InnerText?.Trim();
            if (!string.IsNullOrWhiteSpace(score))
                yield return "評分:" + score;

            if (string.IsNullOrWhiteSpace(callType) && string.IsNullOrWhiteSpace(callerId) && string.IsNullOrWhiteSpace(callerName))
            {
                // 取不到[來電種類]、[來電者]時，取此區塊的第一行問字內容作為總結，如: "目前爲止，這個電話號碼沒有評級。"
                string? title = cardBody.SelectSingleNode(".//div[@id='tellowsscore']")?.NextSibling.InnerText?.Trim();
                if (!string.IsNullOrWhiteSpace(title))
                    yield return title;
            }
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
