using System.ComponentModel.DataAnnotations;

namespace ChurchData.Entities
{
    public class RegistrationRequest
    {
        [Key]
        public Guid RequestId { get; set; }

        [Required]
        [MaxLength(200)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Role { get; set; } = string.Empty;

        public int ParishId { get; set; }

        public int? FamilyId { get; set; }

        [Required]
        [MaxLength(512)]
        public string EmailVerificationToken { get; set; } = string.Empty;

        public bool EmailVerified { get; set; }

        public DateTime? EmailVerifiedAt { get; set; }

        public string? PhoneNumber { get; set; }

        public bool PhoneVerified { get; set; }

        public DateTime? PhoneVerifiedAt { get; set; }

        // Staged phone verification (before user creation)
        public string? PhoneVerificationOtpHash { get; set; }

        public DateTime? PhoneOtpExpiresAt { get; set; }

        public int PhoneOtpAttempts { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime ExpiresAt { get; set; }
    }
}
