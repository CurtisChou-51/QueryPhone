namespace QueryPhone.Web.Models
{
    public class QueryPhoneConditionViewModel
    {
        public string Phone { get; set; } = string.Empty;

        public string[] ClientNames { get; set; } = [];
    }

    public class QueryPhoneResultViewModel
    {
        public string Name { get; set; } = string.Empty;

        public string? QueryUrl { get; set; }

        public bool Success { get; set; }

        /// <summary> 用戶回報 </summary>
        public IEnumerable<string> ReportMsgs { get; set; } = Enumerable.Empty<string>();

        /// <summary> 總評 </summary>
        public IEnumerable<string> SummaryMsgs { get; set; } = Enumerable.Empty<string>();
    }
}
