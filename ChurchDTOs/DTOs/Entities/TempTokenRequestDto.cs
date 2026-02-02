using System.ComponentModel.DataAnnotations;

namespace ChurchDTOs.DTOs.Entities
{
    public class TempTokenRequestDto
    {
        [Required]
        public string TempToken { get; set; } = string.Empty;
    }
}
