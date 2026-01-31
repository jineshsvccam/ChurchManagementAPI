using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ChurchData.Entities;
using Microsoft.AspNetCore.Identity;

namespace ChurchData
{
    public class User : IdentityUser<Guid>
    {
        [Required]
        [Column("fullname")]
        [MaxLength(255)]
        public string FullName { get; set; }

        [Column("family_id")]
        public int? FamilyId { get; set; }

        [Column("parish_id")]
        public int? ParishId { get; set; }


        public UserStatus Status { get; set; } = UserStatus.Pending;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(20)]
        [Column("two_factor_type")]
        public string? TwoFactorType { get; set; }

        [Column("two_factor_enabled_at")]
        public DateTime? TwoFactorEnabledAt { get; set; }

        [Column("first_login_at")]
        public DateTime? FirstLoginAt { get; set; }

        public virtual ICollection<UserRole> UserRoles { get; set; } = new HashSet<UserRole>();
        public virtual Family? Family { get; set; }
        public virtual Parish? Parish { get; set; }
        public virtual ICollection<UserAuthenticator> Authenticators { get; set; } = new HashSet<UserAuthenticator>();
        public virtual ICollection<User2FARecoveryCode> RecoveryCodes { get; set; } = new HashSet<User2FARecoveryCode>();
        public virtual ICollection<User2FASession> TwoFactorSessions { get; set; } = new HashSet<User2FASession>();
        public virtual ICollection<EmailVerificationToken> EmailVerificationTokens { get; set; } = new HashSet<EmailVerificationToken>();
        public virtual ICollection<PhoneVerificationToken> PhoneVerificationTokens { get; set; } = new HashSet<PhoneVerificationToken>();
        public virtual ICollection<PasswordResetToken> PasswordResetTokens { get; set; } = new HashSet<PasswordResetToken>();
    }

    // Email Verification Token
    public class EmailVerificationToken
    {
        [Key]
        [Column("token_id")]
        public Guid TokenId { get; set; } = Guid.NewGuid();

        [Column("user_id")]
        public Guid UserId { get; set; }

        [Column("token")]
        [MaxLength(512)]
        public string Token { get; set; }

        [Column("email")]
        [MaxLength(255)]
        public string Email { get; set; }

        [Column("is_used")]
        public bool IsUsed { get; set; } = false;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("expires_at")]
        public DateTime ExpiresAt { get; set; }

        [Column("verified_at")]
        public DateTime? VerifiedAt { get; set; }

        public virtual User? User { get; set; }
    }

    // Phone Verification Token
    public class PhoneVerificationToken
    {
        [Key]
        [Column("token_id")]
        public Guid TokenId { get; set; } = Guid.NewGuid();

        [Column("user_id")]
        public Guid UserId { get; set; }

        [Column("phone_number")]
        [MaxLength(20)]
        public string PhoneNumber { get; set; }

        [Column("otp")]
        [MaxLength(6)]
        public string Otp { get; set; }

        [Column("is_used")]
        public bool IsUsed { get; set; } = false;

        [Column("attempts")]
        public int Attempts { get; set; } = 0;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("expires_at")]
        public DateTime ExpiresAt { get; set; }

        [Column("verified_at")]
        public DateTime? VerifiedAt { get; set; }

        public virtual User? User { get; set; }
    }

    // Password Reset Token
    public class PasswordResetToken
    {
        [Key]
        [Column("token_id")]
        public Guid TokenId { get; set; } = Guid.NewGuid();

        [Column("user_id")]
        public Guid UserId { get; set; }

        [Column("token")]
        [MaxLength(512)]
        public string Token { get; set; }

        [Column("is_used")]
        public bool IsUsed { get; set; } = false;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("expires_at")]
        public DateTime ExpiresAt { get; set; }

        [Column("used_at")]
        public DateTime? UsedAt { get; set; }

        public virtual User? User { get; set; }
    }

    public class UserRole : IdentityUserRole<Guid>
    {
        [Key, Column(Order = 0)]
        public Guid UserId { get; set; }

        [Key, Column(Order = 1)]
        public Guid RoleId { get; set; }

        [Required]
        public RoleStatus Status { get; set; } = RoleStatus.Pending;

        public Guid? ApprovedBy { get; set; }
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ApprovedAt { get; set; }

        public virtual User? User { get; set; }
        public virtual Role? Role { get; set; }
        public virtual User? ApprovedByUser { get; set; }
    }
    public class Role : IdentityRole<Guid>
    {
        public string ConcurrencyStamp { get; set; } = Guid.NewGuid().ToString();
        public virtual ICollection<UserRole> UserRoles { get; set; } = new HashSet<UserRole>();
    }
    public enum UserStatus
    {
        Active,
        Inactive,
        Suspended,
        Pending
    }

    public enum RoleStatus
    {
        Pending,
        Approved,
        Rejected
    }
}
