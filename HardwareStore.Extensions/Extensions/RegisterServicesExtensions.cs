using HardwareStore.Data.Context;
using HardwareStore.Data.Helper;
using HardwareStore.Data.Models;
using HardwareStore.Data.Models.Interfaces;
using HardwareStore.Data.Models.Repositories;
using HardwareStore.Services.Helpers;
using HardwareStore.Services.Implementations;
using HardwareStore.Services.Interfaces;
using HardwareStore.Services.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HardwareStore.Extensions.Extensions
{
    public static class RegisterServicesExtensions
    {
        public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration config)
        {
            // Add Services to Web Client
            services.AddScoped<INotificationService, NotificationService>();

            // Configure Keycloak options
            services.Configure<KeycloakOptions>(
                config.GetSection("Keycloak"));

            // Add SignalR
            services.AddSignalR();

            return services;
        }

        public static IServiceCollection AddRepositories(this IServiceCollection services, IConfiguration config)
        {
            // Add HttpClient factory first
            services.AddHttpClient();


            services.AddScoped<Keycloak.Net.KeycloakClient>(sp =>
            {
                var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                var httpClient = httpClientFactory.CreateClient();

                var keycloakSection = config.GetSection("Keycloak");
                var serverUrl = keycloakSection["ServerUrl"]!;
                var realm = keycloakSection["Realm"]!;

                var clientId = keycloakSection["ClientId"]!;
                var clientSecret = keycloakSection["ClientSecret"]!;

                var keycloakClient = new Keycloak.Net.KeycloakClient(httpClient.ToString(), serverUrl);

                return keycloakClient;
            });

            // Add Repositories to Web Client
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<INotificationRepository, NotificationRepository>();

            services.AddScoped<KeycloakHelper>();

            // Adds the Sync Service to the application
            services.AddScoped<KeyCloakSync>(sp =>
            {
                var dbContext = sp.GetRequiredService<AppDbContext>();
                var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                var httpClient = httpClientFactory.CreateClient();
                var configuration = sp.GetRequiredService<IConfiguration>();

                var keycloakSection = configuration.GetSection("Keycloak");
                var serverUrl = keycloakSection["ServerUrl"]!;
                var realm = keycloakSection["Realm"]!;
                var adminUser = keycloakSection["AdminUser"]!;
                var adminPassword = keycloakSection["AdminPassword"]!;

                return new KeyCloakSync(dbContext, httpClient, serverUrl, realm, adminUser, adminPassword);
            });

            // Registers the hosted service to sync the users to the SQL database.
            services.AddHostedService<KeyCloakSyncService>();
            return services;
        }
    }
}