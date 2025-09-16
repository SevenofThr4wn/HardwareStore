using HardwareStore.Data.Context;
using HardwareStore.Data.Helper;
using HardwareStore.Data.Repositories;
using HardwareStore.Data.Repositories.Interfaces;
using HardwareStore.Services;
using HardwareStore.Services.Helpers;
using HardwareStore.Services.Implementations;
using HardwareStore.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HardwareStore.Extensions.Extensions
{
    /// <summary>
    /// Extension methods for registering services and repositories
    /// </summary>
    public static class RegisterServicesExtensions
    {
        /// <summary>
        /// Register services for dependency injection
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration config)
        {
            // Add Services to Web Client
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<INotificationService, NotificationService>();

            // Configure Keycloak options
            services.Configure<KeycloakOptions>(
                config.GetSection("Keycloak"));

            // Add SignalR
            services.AddSignalR();

            return services;
        }

        /// <summary>
        /// Register repositories for dependency injection
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        /// <summary>
        /// Register repositories for dependency injection
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddRepositories(this IServiceCollection services, IConfiguration config)
        {
            // Add HttpClient factory first
            services.AddHttpClient();

            // Register KeycloakClient - THIS IS THE MISSING PIECE
            // Register KeycloakClient
            services.AddScoped<Keycloak.Net.KeycloakClient>(sp =>
            {
                var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                var httpClient = httpClientFactory.CreateClient();

                var keycloakSection = config.GetSection("Keycloak");
                var serverUrl = keycloakSection["ServerUrl"]!;
                var realm = keycloakSection["Realm"]!;

                var clientId = keycloakSection["ClientId"]!;
                var clientSecret = keycloakSection["ClientSecret"]!;

                // Create the client with just the base URL first
                var keycloakClient = new Keycloak.Net.KeycloakClient(httpClient.ToString(), serverUrl);

                return keycloakClient;
            });

            // Add Repositories to Web Client
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<INotificationRepository, NotificationRepository>();
            // Remove duplicate registration: services.AddScoped<IProductRepository, ProductRepository>();

            // Add Keycloak Helper - This will now work because KeycloakClient is registered
            services.AddScoped<KeycloakHelper>();

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

            services.AddHostedService<KeycloakUserSyncService>();
            return services;
        }
    }
}