namespace SecurityAssessmentAPI.Models.Entities
{
    public class CheckType
    {
        public int CheckTypeId { get; set; }
        public string Code { get; set; } = string.Empty; // e.g., "SSL_TLS", "Sec_Headers", etc.
        public string Description { get; set; } = string.Empty;

        // Navigation properties
        public ICollection<CheckResult> CheckResults { get; set; } = new List<CheckResult>();
    }
}
