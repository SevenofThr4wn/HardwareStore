using HardwareStore.Services.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HardwareStore.Services.Helpers
{
    public class DatabaseCleanupService : BackgroundService
    {
        private readonly ILogger<DatabaseCleanupService> _logger;
        private readonly IServiceProvider _services;
        private readonly TimeSpan _deletionInterval = TimeSpan.FromDays(5);

        public DatabaseCleanupService(ILogger<DatabaseCleanupService> logger, IServiceProvider services)
        {
            _logger = logger;
            _services = services;
        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _services.CreateScope();
                    var task = scope.ServiceProvider.GetRequiredService<DatabaseCleanup>();
                    await task.DeleteOldOrderRecords(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during record cleanup.");
                }

                await Task.Delay(_deletionInterval, stoppingToken);
            }

            _logger.LogInformation("Record cleanup background service stopped.");

        }
    }
}
