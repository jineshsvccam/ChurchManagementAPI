using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChurchData.Entities
{
    [Table("security_audit_logs")]
    public class SecurityAuditLog
    {
        [Key]
        [Column("log_id")]
        public Guid LogId { get; set; } = Guid.NewGuid();

        [Column("user_id")]
        public Guid? UserId { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("event_type")]
        public string EventType { get; set; } = string.Empty;

        [MaxLength(500)]
        [Column("event_description")]
        public string? EventDescription { get; set; }

        [MaxLength(45)]
        [Column("ip_address")]
        public string? IpAddress { get; set; }

        [MaxLength(512)]
        [Column("user_agent")]
        public string? UserAgent { get; set; }

        [Column("metadata")]
        public string? Metadata { get; set; }

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("severity")]
        [MaxLength(20)]
        public string Severity { get; set; } = "Info";
    }

    /// <summary>
    /// Security event types for audit logging.
    /// </summary>
    public static class SecurityEventType
    {
        // Authentication Events
        public const string LoginSuccess = "LOGIN_SUCCESS";
        public const string LoginFailed = "LOGIN_FAILED";

        // 2FA Setup Events
        public const string TwoFactorSetupInitiated = "2FA_SETUP_INITIATED";
        public const string TwoFactorSetupVerified = "2FA_SETUP_VERIFIED";
        public const string TwoFactorDisabled = "2FA_DISABLED";

        // 2FA Verification Events
        public const string TwoFactorChallengeCreated = "2FA_CHALLENGE_CREATED";
        public const string TwoFactorVerificationSuccess = "2FA_VERIFICATION_SUCCESS";
        public const string TwoFactorVerificationFailed = "2FA_VERIFICATION_FAILED";
        public const string TwoFactorMaxAttemptsExceeded = "2FA_MAX_ATTEMPTS_EXCEEDED";
        public const string TwoFactorSessionExpired = "2FA_SESSION_EXPIRED";
        public const string TwoFactorReplayAttempt = "2FA_REPLAY_ATTEMPT";

        // Recovery Code Events
        public const string RecoveryCodeUsed = "2FA_RECOVERY_CODE_USED";
        public const string RecoveryCodesGenerated = "2FA_RECOVERY_CODES_GENERATED";

        // Rate Limiting Events
        public const string TwoFactorRateLimitExceeded = "2FA_RATE_LIMIT_EXCEEDED";

        // Session Security Events
        public const string TwoFactorFingerprintMismatch = "2FA_FINGERPRINT_MISMATCH";

        // Configuration Events
        public const string TwoFactorGlobalConfigChanged = "2FA_GLOBAL_CONFIG_CHANGED";
    }

    /// <summary>
    /// Severity levels for audit logs.
    /// </summary>
    public static class AuditSeverity
    {
        public const string Info = "Info";
        public const string Warning = "Warning";
        public const string Critical = "Critical";
    }
}
