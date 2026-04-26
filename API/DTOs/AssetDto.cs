namespace SecurityAssessmentAPI.DTOs
{
    public class AssetDto
    {
        public int AssetId { get; set; }
        public string AssetType { get; set; } = string.Empty; // "Domain" or "Ip"
        public string Value { get; set; } = string.Empty;
        public List<AssessmentRunDto> AssessmentRuns { get; set; } = new List<AssessmentRunDto>();
    }
}
