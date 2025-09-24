using HardwareStore.Services.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HardwareStore.Services.Helpers
{
    public class KeyCloakSyncService : BackgroundService
    {

        private readonly ILogger<KeyCloakSyncService> _logger;
        private readonly IServiceProvider _services;
        private readonly TimeSpan _syncInterval = TimeSpan.FromMinutes(2);

        public KeyCloakSyncService(ILogger<KeyCloakSyncService> logger, IServiceProvider services)
        {
            _logger = logger;
            _services = services;
        }

        /// <summary>
        /// Periodically synchronizes users from Keycloak by invoking the KeyCloakSync service.
        /// Runs until the service is stopped or a cancellation is requested.
        /// </summary>
        /// <param name="stoppingToken">Token to monitor for cancellation requests.</param>
        /// <returns>A Task representing the background operation.</returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Keycloak User Sync Service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _services.CreateScope())
                    {
                        var syncService = scope.ServiceProvider.GetRequiredService<KeyCloakSync>();
                        await syncService.SyncUsersAsync();
                        _logger.LogInformation("Keycloak users synced at {Time}", DateTimeOffset.Now);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error syncing Keycloak users.");
                }

                await Task.Delay(_syncInterval, stoppingToken);
            }

            _logger.LogInformation("Keycloak User Sync Service stopped.");
        }
    }
}