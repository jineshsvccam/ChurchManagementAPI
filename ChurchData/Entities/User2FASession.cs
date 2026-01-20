using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChurchData.Entities
{
    [Table("user_2fa_sessions")]
    public class User2FASession
    {
        [Key]
        [Column("session_id")]
        public Guid SessionId { get; set; }

        [Required]
        [Column("user_id")]
        public Guid UserId { get; set; }

        [Required]
        [MaxLength(512)]
        [Column("temp_token")]
        public string TempToken { get; set; } = string.Empty;

        [Column("ip_address")]
        public string? IpAddress { get; set; }

        [Column("user_agent")]
        public string? UserAgent { get; set; }

        /// <summary>
        /// SHA256 hash of client fingerprint (IP + User-Agent).
        /// Used for MITM protection - session bound to original client.
        /// </summary>
        [MaxLength(64)]
        [Column("client_fingerprint")]
        public string? ClientFingerprint { get; set; }

        [Required]
        [Column("attempts")]
        public short Attempts { get; set; } = 0;

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [Column("expires_at")]
        public DateTime ExpiresAt { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual User? User { get; set; }
    }
}
