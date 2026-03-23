namespace SecurityAssessmentAPI.DTOs
{
    public class CustomerDto
    {
        public int CustomerId { get; set; }
        public string Name { get; set; }
        public string Telefon { get; set; }
        public string Email { get; set; }
        public List<AssetDto> Assets { get; set; } = new List<AssetDto>();
    }
}
