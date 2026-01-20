using ChurchData.Entities;

namespace ChurchContracts.Interfaces.Services
{
    /// <summary>
    /// Service for logging security audit events.
    /// All methods are fire-and-forget to avoid blocking authentication flows.
    /// </summary>
    public interface ISecurityAuditService
    {
        /// <summary>
        /// Logs a security audit event asynchronously without blocking the caller.
        /// </summary>
        Task LogAsync(
            string eventType,
            Guid? userId,
            string? description = null,
            string? ipAddress = null,
            string? userAgent = null,
            string severity = AuditSeverity.Info,
            Dictionary<string, string>? metadata = null);

        /// <summary>
        /// Logs a security audit event without awaiting completion.
        /// Use this in critical paths where logging must not block.
        /// </summary>
        void LogFireAndForget(
            string eventType,
            Guid? userId,
            string? description = null,
            string? ipAddress = null,
            string? userAgent = null,
            string severity = AuditSeverity.Info,
            Dictionary<string, string>? metadata = null);
    }
}
