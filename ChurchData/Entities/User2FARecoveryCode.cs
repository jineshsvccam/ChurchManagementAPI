using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChurchData.Entities
{
    [Table("user_2fa_recovery_codes")]
    public class User2FARecoveryCode
    {
        [Key]
        [Column("recovery_code_id")]
        public Guid RecoveryCodeId { get; set; }

        [Required]
        [Column("user_id")]
        public Guid UserId { get; set; }

        [Required]
        [MaxLength(256)]
        [Column("recovery_code_hash")]
        public string RecoveryCodeHash { get; set; } = string.Empty;

        [Required]
        [Column("is_used")]
        public bool IsUsed { get; set; } = false;

        [Column("used_at")]
        public DateTime? UsedAt { get; set; }

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(UserId))]
        public virtual User? User { get; set; }
    }
}
