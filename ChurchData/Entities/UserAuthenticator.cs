using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChurchData.Entities
{
    [Table("user_authenticators")]
    public class UserAuthenticator
    {
        [Key]
        [Column("authenticator_id")]
        public Guid AuthenticatorId { get; set; }

        [Required]
        [Column("user_id")]
        public Guid UserId { get; set; }

        [Required]
        [MaxLength(256)]
        [Column("secret_key")]
        public string SecretKey { get; set; } = string.Empty;

        [Required]
        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("verified_at")]
        public DateTime? VerifiedAt { get; set; }

        /// <summary>
        /// Last TOTP time step that was successfully used.
        /// Used for replay attack prevention - codes from same/older time step are rejected.
        /// </summary>
        [Column("last_used_time_step")]
        public long? LastUsedTimeStep { get; set; }

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("revoked_at")]
        public DateTime? RevokedAt { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual User? User { get; set; }
    }
}
