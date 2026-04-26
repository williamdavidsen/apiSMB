namespace SecurityAssessmentAPI.Models.Entities
{
    public enum CheckResultStatus
    {
        OK,
        Warning,
        Critical,
        Error,
        NotAvailable
    }

    public class CheckResult
    {
        public int CheckResultId { get; set; }
        public int CheckTypeId { get; set; }
        public int RunId { get; set; }
        public decimal ScorePart { get; set; } // Score for this check (e.g., SSL_TLS score)
        public CheckResultStatus Status { get; set; }
        public string RawPayload { get; set; } = string.Empty; // JSON - Stores the API response as is
        public string NormalizedData { get; set; } = string.Empty; // JSON - Simplified data for the dashboard

        // Navigation properties
        public CheckType CheckType { get; set; } = null!;
        public AssessmentRun AssessmentRun { get; set; } = null!;
        public ICollection<Finding> Findings { get; set; } = new List<Finding>();
    }
}
