using System.Text.Json;
using ChurchContracts.Interfaces.Services;
using ChurchData;
using ChurchData.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ChurchServices.Security
{
    /// <summary>
    /// PostgreSQL-based security audit logging service.
    /// Designed to be non-blocking and fail-safe.
    /// </summary>
    public class SecurityAuditService : ISecurityAuditService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<SecurityAuditService> _logger;

        public SecurityAuditService(
            IServiceScopeFactory scopeFactory,
            ILogger<SecurityAuditService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task LogAsync(
            string eventType,
            Guid? userId,
            string? description = null,
            string? ipAddress = null,
            string? userAgent = null,
            string severity = AuditSeverity.Info,
            Dictionary<string, string>? metadata = null)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var auditLog = new SecurityAuditLog
                {
                    LogId = Guid.NewGuid(),
                    UserId = userId,
                    EventType = eventType,
                    EventDescription = description,
                    IpAddress = SanitizeIpAddress(ipAddress),
                    UserAgent = SanitizeUserAgent(userAgent),
                    Severity = severity,
                    Metadata = metadata != null ? JsonSerializer.Serialize(metadata) : null,
                    CreatedAt = DateTime.UtcNow
                };

                context.SecurityAuditLogs.Add(auditLog);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log to application logger but never throw - audit logging must not block auth
                _logger.LogError(ex, 
                    "Failed to write security audit log. EventType: {EventType}, UserId: {UserId}",
                    eventType, userId);
            }
        }

        public void LogFireAndForget(
            string eventType,
            Guid? userId,
            string? description = null,
            string? ipAddress = null,
            string? userAgent = null,
            string severity = AuditSeverity.Info,
            Dictionary<string, string>? metadata = null)
        {
            // Fire and forget - do not await
            _ = Task.Run(async () =>
            {
                try
                {
                    await LogAsync(eventType, userId, description, ipAddress, userAgent, severity, metadata);
                }
                catch (Exception ex)
                {
                    // Ensure no exception escapes the background task
                    _logger.LogError(ex, 
                        "Background security audit log failed. EventType: {EventType}",
                        eventType);
                }
            });
        }

        /// <summary>
        /// Sanitizes IP address to prevent log injection and limit length.
        /// </summary>
        private static string? SanitizeIpAddress(string? ipAddress)
        {
            if (string.IsNullOrEmpty(ipAddress))
                return null;

            // Remove any potentially dangerous characters and limit length
            var sanitized = ipAddress
                .Replace("\n", "")
                .Replace("\r", "")
                .Replace("\t", "");

            return sanitized.Length > 45 ? sanitized[..45] : sanitized;
        }

        /// <summary>
        /// Sanitizes User-Agent to prevent log injection and limit length.
        /// </summary>
        private static string? SanitizeUserAgent(string? userAgent)
        {
            if (string.IsNullOrEmpty(userAgent))
                return null;

            // Remove any potentially dangerous characters and limit length
            var sanitized = userAgent
                .Replace("\n", "")
                .Replace("\r", "")
                .Replace("\t", "");

            return sanitized.Length > 512 ? sanitized[..512] : sanitized;
        }
    }
}
