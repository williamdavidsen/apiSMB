namespace SecurityAssessmentAPI.DTOs
{
    public class AssessmentRunCreateDto
    {
        public int AssetId { get; set; }
        public DateTime StartedAt { get; set; }
    }

    public class AssessmentRunDto
    {
        public int RunId { get; set; }
        public int AssetId { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }
        public string Status { get; set; } = string.Empty; // "Pending", "Running", "Success", "Partial", "Failed"
        public int SummaryScore { get; set; } // 0-100
        public string Grade { get; set; } = string.Empty; // "A", "B", "C", "D", "E", "F"
        public List<CheckResultDto> CheckResults { get; set; } = new List<CheckResultDto>();
    }
}
