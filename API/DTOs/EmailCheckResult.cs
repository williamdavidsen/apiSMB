namespace SecurityAssessmentAPI.DTOs
{
    public class EmailCheckResult
    {
        public string Domain { get; set; } = string.Empty;
        public bool HasMailService { get; set; }
        public bool ModuleApplicable { get; set; } = true;
        public int OverallScore { get; set; }
        public int MaxScore { get; set; } = 20;
        public string Status { get; set; } = "UNKNOWN";
        public EmailCriteria Criteria { get; set; } = new();
        public EmailDnsSummary DnsSummary { get; set; } = new();
        public List<EmailAlert> Alerts { get; set; } = new();
    }

    public class EmailCriteria
    {
        public EmailScoreDetail SpfVerification { get; set; } = new();
        public EmailScoreDetail DkimActivated { get; set; } = new();
        public EmailScoreDetail DmarcEnforcement { get; set; } = new();
    }

    public class EmailScoreDetail
    {
        public int Score { get; set; }
        public string Confidence { get; set; } = "HIGH";
        public string Details { get; set; } = string.Empty;
    }

    public class EmailDnsSummary
    {
        public List<string> MxRecords { get; set; } = new();
        public string SpfRecord { get; set; } = string.Empty;
        public string DmarcRecord { get; set; } = string.Empty;
        public List<string> DkimSelectorsFound { get; set; } = new();
    }

    public class EmailAlert
    {
        public string Type { get; set; } = "INFO";
        public string Message { get; set; } = string.Empty;
    }
}
