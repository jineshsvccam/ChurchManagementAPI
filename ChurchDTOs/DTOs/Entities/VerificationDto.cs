using System.ComponentModel.DataAnnotations;

namespace ChurchDTOs.DTOs.Entities
{
    // Email Verification
    public class SendEmailVerificationDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }

    public class VerifyEmailDto
    {
        [Required]
        [MaxLength(512)]
        public string Token { get; set; }
    }

    public class EmailVerificationResponseDto
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
    }

    // Phone Verification
    public class SendPhoneVerificationDto
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        [Phone]
        public string PhoneNumber { get; set; }
    }

    public class VerifyPhoneDto
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        [StringLength(6, MinimumLength = 6)]
        public string Otp { get; set; }
    }

    public class PhoneVerificationResponseDto
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
    }

    // Password Reset
    public class ForgotPasswordDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }

    public class ResetPasswordDto
    {
        [Required]
        [MaxLength(512)]
        public string Token { get; set; }

        [Required]
        [MinLength(8)]
        public string NewPassword { get; set; }

        [Required]
        [Compare("NewPassword")]
        public string ConfirmPassword { get; set; }
    }

    public class ResetPasswordResponseDto
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
    }

    public class PasswordResetRequestResponseDto
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
    }
}
