using System.ComponentModel.DataAnnotations;

namespace ChurchDTOs.DTOs.Entities
{
    public class RegisterRequestDto
    {
        [Required]
        [MaxLength(200)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Role { get; set; } = string.Empty;

        [Required]
        public int ParishId { get; set; }

        public int? FamilyId { get; set; }
    }

    public class RegisterRequestResponseDto
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;

        // For UI navigation only. Not sensitive.
        public string? Token { get; set; }
    }

    public class VerifyRegistrationEmailDto
    {
        [Required]
        [MaxLength(512)]
        public string Token { get; set; } = string.Empty;
    }

    public class VerifyRegistrationEmailResponseDto
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class CompleteRegistrationDto
    {
        [Required]
        [MaxLength(512)]
        public string Token { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        public string Password { get; set; } = string.Empty;

        [Phone]
        [MaxLength(30)]
        public string? PhoneNumber { get; set; }
    }

    public class CompleteRegistrationResponseDto
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
