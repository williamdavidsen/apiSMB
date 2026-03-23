namespace SecurityAssessmentAPI.Models.Entities
{
    public class Customer
    {
        public int CustomerId { get; set; }
        public string Name { get; set; }
        public string Telefon { get; set; }
        public string Email { get; set; }

        // Navigation properties
        public ICollection<Asset> Assets { get; set; } = new List<Asset>();
    }
}
