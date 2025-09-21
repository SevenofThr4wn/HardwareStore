using HardwareStore.Data.Helper;
using HardwareStore.Data.Context;
using HardwareStore.Data.Repositories;
using HardwareStore.Data.Repositories.Interfaces;
using HardwareStore.Services;
using HardwareStore.Services.Helpers;
using HardwareStore.Services.Interfaces;
using HardwareStore.Services.Implementations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HardwareStore.Extensions.Extensions
{
    public static class RegisterServicesExtensions
    {
        public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration config)
        {
            // Add Services to Web Client
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IProductService, ProductService>();
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
            services.AddScoped<KeycloakSync>(sp =>
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

                return new KeycloakSync(dbContext, httpClient, serverUrl, realm, adminUser, adminPassword);
            });

            // Registers the hosted service to sync the users to the SQL database.
            services.AddHostedService<KeycloakUserSyncService>();
            return services;
        }
    }
}