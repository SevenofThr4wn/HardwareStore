using HardwareStore.Data.Helper;
using HardwareStore.Data.Repositories;
using HardwareStore.Data.Repositories.Interfaces;
using HardwareStore.Services.Interfaces;
using HardwareStore.Services.Implementations;
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
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            // Add Repositories to Web Client
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();

            // Add Keycloak Helper
            services.AddScoped<KeycloakHelper>();
            return services;
        }
    }
}