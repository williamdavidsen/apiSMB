namespace SecurityAssessmentAPI.Models.Entities
{
    public enum AssetType
    {
        Domain,
        Ip
    }

    public class Asset
    {
        public int AssetId { get; set; }
        public int CustomerId { get; set; }
        public AssetType AssetType { get; set; }
        public string Value { get; set; }

        // Navigation properties
        public Customer Customer { get; set; }
        public ICollection<AssessmentRun> AssessmentRuns { get; set; } = new List<AssessmentRun>();
    }
}
