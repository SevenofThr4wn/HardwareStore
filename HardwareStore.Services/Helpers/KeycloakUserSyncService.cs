using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HardwareStore.Services.Helpers
{
    public class KeycloakUserSyncService : BackgroundService
    {

        private readonly ILogger<KeycloakUserSyncService> _logger;
        private readonly IServiceProvider _services;
        private readonly TimeSpan _syncInterval = TimeSpan.FromMinutes(2);

        public KeycloakUserSyncService(ILogger<KeycloakUserSyncService> logger, IServiceProvider services)
        {
            _logger = logger;
            _services = services;
        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Keycloak User Sync Service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _services.CreateScope())
                    {
                        var syncService = scope.ServiceProvider.GetRequiredService<KeycloakSync>();
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
