using Microsoft.Extensions.Logging;
using QueryPhone.Core.Models;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace QueryPhone.Core.Clients
{
    public class WhosNumberClient : IQueryPhoneClient
    {
        private readonly ILogger _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        private string QueryUrl(string phone) => $"https://whosnumber.com/tw/{phone}";

        public WhosNumberClient(ILogger<WhosNumberClient> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public string Name => "WhosNumber 到底是誰打來的電話";

        public async Task<QueryPhoneResult> QueryAsync(string phone)
        {
            try
            {
                var resp = await QueryImpl(phone);
                string respStr = await resp.Content.ReadAsStringAsync();
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(respStr);

                var summaryMsgs = YieldSummaryMsgs(doc).Distinct().ToList();
                var reportMsgs = YieldReportMsgs(doc).ToList();
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

        /// <summary> 提取總評文字 </summary>
        private static IEnumerable<string> YieldSummaryMsgs(HtmlDocument doc)
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

        /// <summary> 提取用戶回報文字 </summary>
        private static IEnumerable<string> YieldReportMsgs(HtmlDocument doc)
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

        private Task<HttpResponseMessage> QueryImpl(string phone)
        {
            var client = _httpClientFactory.CreateClient();
            var req = new HttpRequestMessage(HttpMethod.Get, QueryUrl(phone));
            return client.SendAsync(req);
        }
    }
}
