namespace QueryPhone.Model
{
    public class QueryPhoneResult
    {
        public string? QueryUrl { get; set; }

        public bool Success { get; set; }

        public IEnumerable<string> ReportMsgs { get; set; } = Enumerable.Empty<string>();

        public IEnumerable<string> SummaryMsgs { get; set; } = Enumerable.Empty<string>();
    }
}
