namespace SecurityAssessmentAPI.Models.Entities
{
    public enum Severity
    {
        Low,
        Medium,
        High,
        Critical
    }

    public class Finding
    {
        public int ReasonId { get; set; }
        public int CheckResultId { get; set; }
        public Severity Severity { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Evidence { get; set; } = string.Empty;

        // Navigation properties
        public CheckResult CheckResult { get; set; } = null!;
    }
}
