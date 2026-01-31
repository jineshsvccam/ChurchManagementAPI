using System.ComponentModel.DataAnnotations;

namespace ChurchDTOs.DTOs.Entities
{
    public class SendRegistrationPhoneOtpDto
    {
        [Required]
        [MaxLength(512)]
        public string Token { get; set; } = string.Empty;

        [Required]
        [Phone]
        [MaxLength(30)]
        public string PhoneNumber { get; set; } = string.Empty;
    }

    public class VerifyRegistrationPhoneOtpDto
    {
        [Required]
        [MaxLength(512)]
        public string Token { get; set; } = string.Empty;

        [Required]
        [StringLength(6, MinimumLength = 6)]
        public string Otp { get; set; } = string.Empty;
    }

    public class RegistrationPhoneVerificationResponseDto
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;

        public bool PhoneVerified { get; set; }
    }
}
