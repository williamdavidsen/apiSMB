namespace SecurityAssessmentAPI.DTOs
{
    public class AssessmentCheckResult
    {
        public string Domain { get; set; } = string.Empty;
        public int OverallScore { get; set; }
        public int MaxScore { get; set; } = 100;
        public string Status { get; set; } = "UNKNOWN";
        public string Grade { get; set; } = "F";
        public bool EmailModuleIncluded { get; set; }
        public PqcCheckResult PqcReadiness { get; set; } = new();
        public AssessmentWeights Weights { get; set; } = new();
        public AssessmentModuleScores Modules { get; set; } = new();
        public List<AssessmentAlert> Alerts { get; set; } = new();
    }

    public class AssessmentWeights
    {
        public decimal SslTls { get; set; }
        public decimal HttpHeaders { get; set; }
        public decimal EmailSecurity { get; set; }
        public decimal Reputation { get; set; }
    }

    public class AssessmentModuleScores
    {
        public AssessmentModuleScore SslTls { get; set; } = new();
        public AssessmentModuleScore HttpHeaders { get; set; } = new();
        public AssessmentModuleScore EmailSecurity { get; set; } = new();
        public AssessmentModuleScore Reputation { get; set; } = new();
    }

    public class AssessmentModuleScore
    {
        public bool Included { get; set; }
        public decimal WeightPercent { get; set; }
        public int RawScore { get; set; }
        public int RawMaxScore { get; set; }
        public decimal NormalizedScore { get; set; }
        public decimal WeightedContribution { get; set; }
        public string Status { get; set; } = "UNKNOWN";
    }

    public class AssessmentAlert
    {
        public string Type { get; set; } = "INFO";
        public string Message { get; set; } = string.Empty;
    }
}
