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
        public string Title { get; set; }
        public string Description { get; set; }
        public string Evidence { get; set; }

        // Navigation properties
        public CheckResult CheckResult { get; set; }
    }
}
