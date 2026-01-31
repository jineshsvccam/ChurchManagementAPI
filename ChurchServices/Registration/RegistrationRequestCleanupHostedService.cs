using ChurchData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ChurchServices.Registration
{
    /// <summary>
    /// Periodically deletes expired rows from registration_requests.
    /// Security: reduces the volume of valid tokens stored and limits the blast radius of a DB leak.
    /// </summary>
    public class RegistrationRequestCleanupHostedService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<RegistrationRequestCleanupHostedService> _logger;

        public RegistrationRequestCleanupHostedService(IServiceScopeFactory scopeFactory, ILogger<RegistrationRequestCleanupHostedService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Run every 15 minutes.
            var interval = TimeSpan.FromMinutes(15);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    var now = DateTime.UtcNow;
                    var deleted = await db.RegistrationRequests
                        .Where(r => r.ExpiresAt < now)
                        .ExecuteDeleteAsync(stoppingToken);

                    if (deleted > 0)
                    {
                        _logger.LogInformation("Deleted {Count} expired registration requests", deleted);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error cleaning up expired registration requests");
                }

                await Task.Delay(interval, stoppingToken);
            }
        }
    }
}
