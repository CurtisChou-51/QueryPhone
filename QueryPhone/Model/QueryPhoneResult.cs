namespace QueryPhone.Model
{
    public class QueryPhoneResult
    {
        public string? QueryUrl { get; set; }

        public bool Success { get; set; }

        /// <summary> 用戶回報 </summary>
        public IEnumerable<string> ReportMsgs { get; set; } = Enumerable.Empty<string>();

        /// <summary> 總評 </summary>
        public IEnumerable<string> SummaryMsgs { get; set; } = Enumerable.Empty<string>();
    }
}
