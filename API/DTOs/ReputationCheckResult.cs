namespace SecurityAssessmentAPI.DTOs
{
    public class ReputationCheckResult
    {
        public string Domain { get; set; } = string.Empty;
        public int OverallScore { get; set; }
        public int MaxScore { get; set; } = 20;
        public string Status { get; set; } = "UNKNOWN";
        public ReputationCriteria Criteria { get; set; } = new();
        public ReputationSummary Summary { get; set; } = new();
        public List<ReputationAlert> Alerts { get; set; } = new();
    }

    public class ReputationCriteria
    {
        public ReputationScoreDetail BlacklistStatus { get; set; } = new();
        public ReputationScoreDetail MalwareAssociation { get; set; } = new();
    }

    public class ReputationScoreDetail
    {
        public int Score { get; set; }
        public string Confidence { get; set; } = "HIGH";
        public string Details { get; set; } = string.Empty;
    }

    public class ReputationSummary
    {
        public int MaliciousDetections { get; set; }
        public int SuspiciousDetections { get; set; }
        public int HarmlessDetections { get; set; }
        public int UndetectedDetections { get; set; }
        public int Reputation { get; set; }
        public int CommunityMaliciousVotes { get; set; }
        public int CommunityHarmlessVotes { get; set; }
        public string LastAnalysisDate { get; set; } = string.Empty;
        public string Permalink { get; set; } = string.Empty;
    }

    public class ReputationAlert
    {
        public string Type { get; set; } = "INFO";
        public string Message { get; set; } = string.Empty;
    }
}
