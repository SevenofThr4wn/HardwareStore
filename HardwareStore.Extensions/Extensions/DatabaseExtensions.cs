using HardwareStore.Data.Context;
using HardwareStore.Data.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HardwareStore.Extensions.Extensions
{
    public static class DatabaseExtensions
    {
        public static IServiceCollection ConfigureSQLDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            // Configures the DbContext to use SQL Server with the connection string from configuration
            services.AddDbContext<AppDbContext>(opts =>
                 opts.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            return services;
        }

        public static IServiceCollection AddIdentityConfig(this IServiceCollection services)
        {
            // Configures Identity with default token providers and Entity Framework stores
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>();

            return services;
        }
    }
}