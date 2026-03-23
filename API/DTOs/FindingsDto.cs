namespace SecurityAssessmentAPI.DTOs
{
    public class FindingsDto
    {
        public int ReasonId { get; set; }
        public int CheckResultId { get; set; }
        public string Severity { get; set; } // "Low", "Medium", "High", "Critical"
        public string Title { get; set; }
        public string Description { get; set; }
        public string Evidence { get; set; }
    }
}
