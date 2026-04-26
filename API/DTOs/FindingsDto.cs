namespace SecurityAssessmentAPI.DTOs
{
    public class FindingsDto
    {
        public int ReasonId { get; set; }
        public int CheckResultId { get; set; }
        public string Severity { get; set; } = string.Empty; // "Low", "Medium", "High", "Critical"
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Evidence { get; set; } = string.Empty;
    }
}
