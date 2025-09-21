using HardwareStore.Core.Models;
using HardwareStore.Data.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HardwareStore.Extensions.Extensions
{
    public static class DatabaseExtensions
    {

        /// <summary>
        /// Configures the web application's SQL Server database context using the provided configuration.
        /// </summary>
        /// <param name="services">The service collection to add the DbContext to.</param>
        /// <param name="configuration">The application configuration containing the connection string.</param>
        /// <returns>The updated IServiceCollection for chaining.</returns>
        public static IServiceCollection ConfigureSQLDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            // Configures the DbContext to use SQL Server with the connection string from configuration
            services.AddDbContext<AppDbContext>(opts =>
                 opts.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            return services;
        }


        /// <summary>
        /// Configures ASP.NET Core Identity with default token providers and Entity Framework stores
        /// using the application's AppDbContext and ApplicationUser.
        /// </summary>
        /// <param name="services">The IServiceCollection to add Identity services to.</param>
        /// <returns>The IServiceCollection for chaining.</returns>
        public static IServiceCollection AddIdentityConfig(this IServiceCollection services)
        {
            // Configures Identity with default token providers and Entity Framework stores
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>();

            return services;
        }
    }
}