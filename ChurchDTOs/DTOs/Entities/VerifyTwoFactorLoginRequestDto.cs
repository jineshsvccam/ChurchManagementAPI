using System.ComponentModel.DataAnnotations;

namespace ChurchDTOs.DTOs.Entities
{
    public class VerifyTwoFactorLoginRequestDto
    {
        [Required]
        public string TempToken { get; set; } = string.Empty;

        [Required]
        [StringLength(10, MinimumLength = 6)]
        public string Code { get; set; } = string.Empty;
    }
}
