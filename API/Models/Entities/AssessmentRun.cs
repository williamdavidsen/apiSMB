namespace SecurityAssessmentAPI.Models.Entities
{
    public enum AssessmentStatus
    {
        Pending,
        Running,
        Success,
        Partial,
        Failed
    }

    public enum Grade
    {
        A,
        B,
        C,
        D,
        E,
        F
    }

    public class AssessmentRun
    {
        public int RunId { get; set; }
        public int AssetId { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }
        public AssessmentStatus Status { get; set; }
        public int SummaryScore { get; set; } // 0-100
        public Grade Grade { get; set; }

        // Navigation properties
        public Asset Asset { get; set; } = null!;
        public ICollection<CheckResult> CheckResults { get; set; } = new List<CheckResult>();
    }
}
