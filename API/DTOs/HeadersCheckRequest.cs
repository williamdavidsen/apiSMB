using System.ComponentModel.DataAnnotations;

namespace SecurityAssessmentAPI.DTOs
{
    public class HeadersCheckRequest
    {
        [Required]
        [StringLength(253, MinimumLength = 1)]
        public string Domain { get; set; } = string.Empty;
    }
}
