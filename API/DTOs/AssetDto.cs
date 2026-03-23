namespace SecurityAssessmentAPI.DTOs
{
    public class AssetDto
    {
        public int AssetId { get; set; }
        public int CustomerId { get; set; }
        public string AssetType { get; set; } // "Domain" or "Ip"
        public string Value { get; set; }
        public List<AssessmentRunDto> AssessmentRuns { get; set; } = new List<AssessmentRunDto>();
    }
}
