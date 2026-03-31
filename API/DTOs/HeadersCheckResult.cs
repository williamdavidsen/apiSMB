namespace SecurityAssessmentAPI.DTOs
{
    public class HeadersCheckResult
    {
        public string Domain { get; set; } = string.Empty;
        public int OverallScore { get; set; }
        public int MaxScore { get; set; } = 10;
        public string Status { get; set; } = "UNKNOWN";
        public HeadersCriteria Criteria { get; set; } = new();
        public HeadersObservatorySummary Observatory { get; set; } = new();
        public List<HeadersAlert> Alerts { get; set; } = new();
    }

    public class HeadersCriteria
    {
        public HeaderScoreDetail StrictTransportSecurity { get; set; } = new();
        public HeaderScoreDetail ContentSecurityPolicy { get; set; } = new();
        public HeaderScoreDetail ClickjackingProtection { get; set; } = new();
        public HeaderScoreDetail MimeSniffingProtection { get; set; } = new();
        public HeaderScoreDetail ReferrerPolicy { get; set; } = new();
    }

    public class HeaderScoreDetail
    {
        public int Score { get; set; }
        public string Details { get; set; } = string.Empty;
    }

    public class HeadersObservatorySummary
    {
        public string Grade { get; set; } = "UNKNOWN";
        public int Score { get; set; }
        public int TestsPassed { get; set; }
        public int TestsFailed { get; set; }
        public int TestsQuantity { get; set; }
        public string DetailsUrl { get; set; } = string.Empty;
    }

    public class HeadersAlert
    {
        public string Type { get; set; } = "INFO";
        public string Message { get; set; } = string.Empty;
    }
}
