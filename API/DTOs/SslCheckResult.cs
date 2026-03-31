namespace SecurityAssessmentAPI.DTOs
{
    public class SslCheckResult
    {
        public string Domain { get; set; } = string.Empty;
        public int OverallScore { get; set; }
        public int MaxScore { get; set; } = 30;
        public string Status { get; set; } = "UNKNOWN"; // PASS, FAIL, WARNING
        public SslCriteria Criteria { get; set; } = new();
        public List<SslAlert> Alerts { get; set; } = new();
    }

    public class SslCriteria
    {
        public SslScoreDetail TlsVersion { get; set; } = new();
        public SslScoreDetail CertificateValidity { get; set; } = new();
        public SslScoreDetail RemainingLifetime { get; set; } = new();
        public SslScoreDetail CipherStrength { get; set; } = new();
    }

    public class SslScoreDetail
    {
        public int Score { get; set; }
        public string Details { get; set; } = string.Empty;
    }

    public class SslAlert
    {
        public string Type { get; set; } = "INFO"; // CRITICAL_WARNING, CRITICAL_ALARM, INFO
        public string Message { get; set; } = string.Empty;
        public DateTime? ExpiryDate { get; set; }
    }
}