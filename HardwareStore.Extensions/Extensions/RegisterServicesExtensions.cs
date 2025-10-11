using HardwareStore.Data.Context;
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
    /// <summary>
    /// An extension class to configure the required repositories and services for the web interfaces
    /// </summary>
    public static class RegisterServicesExtensions
    {
        /// <summary>
        /// An extension method that configures and adds the required services to the web client.
        /// </summary>
        /// <param name="services">An extension of the IServiceCollection interface.</param>
        /// <param name="config">An extension of the IConfiguration interface.</param>
        /// <returns>The IServiceCollection for chaining.</returns>
        public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddScoped<INotificationService, NotificationService>();
            services.AddSignalR();

            return services;
        }

        /// <summary>
        /// An extension method thar configures and adds the required repositories and unit of works to the web client.
        /// </summary>
        /// <param name="services">An extension of the IServiceCollection interface.</param>
        /// <param name="config">An extension of the IConfiguration interface.</param>
        /// <returns>The IServiceCollection for chaining.</returns>
        public static IServiceCollection AddRepositories(this IServiceCollection services, IConfiguration config)
        {
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

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

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

            services.AddHostedService<KeyCloakSyncService>();
            return services;
        }
    }
}