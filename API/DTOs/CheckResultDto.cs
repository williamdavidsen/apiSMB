namespace SecurityAssessmentAPI.DTOs
{
    public class CheckResultDto
    {
        public int CheckResultId { get; set; }
        public int CheckTypeId { get; set; }
        public int RunId { get; set; }
        public decimal ScorePart { get; set; }
        public string Status { get; set; } = string.Empty; // "OK", "Warning", "Critical", "Error", "NotAvailable"
        public string RawPayload { get; set; } = string.Empty; // JSON
        public string NormalizedData { get; set; } = string.Empty; // JSON
        public CheckTypeDto? CheckType { get; set; }
        public List<FindingsDto> Findings { get; set; } = new List<FindingsDto>();
    }
}
