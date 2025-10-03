using HardwareStore.Core.Models;
using HardwareStore.Data.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HardwareStore.Extensions.Extensions
{
    /// <summary>
    /// Provides extension methods for configuring the SQL Server database
    /// and ASP.NET Core Identity services for the HardwareStore application.
    /// </summary>
    /// <remarks>
    /// These extensions are intended to be called from <c>Program.cs</c>
    /// during application initialization to register database and identity services
    /// with the dependency injection (DI) container.
    /// </remarks>
    public static class DatabaseExtensions
    {

        /// <summary>
        /// Configures the <see cref="AppDbContext"/> to use SQL Server with the
        /// connection string provided in the application configuration.
        /// </summary>
        /// <param name="services">The service collection to register the <see cref="AppDbContext"/> into.</param>
        /// <param name="configuration">
        /// The application configuration, expected to contain a connection string named <c>DefaultConnection</c>.
        /// </param>
        /// <returns>The updated <see cref="IServiceCollection"/> to allow for method chaining.</returns>
        /// <example>
        /// Example usage in <c>Program.cs</c>:
        /// <code>
        /// builder.Services.ConfigureSQLDatabase(builder.Configuration);
        /// </code>
        /// </example>
        /// <remarks>
        /// This registers <see cref="AppDbContext"/> with scoped lifetime by default.
        /// </remarks>
        public static IServiceCollection ConfigureSQLDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            // Configures the DbContext to use SQL Server with the connection string from configuration
            services.AddDbContext<AppDbContext>(opts =>
                 opts.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            return services;
        }


        /// <summary>
        /// Configures ASP.NET Core Identity with the default user and role stores,
        /// using <see cref="ApplicationUser"/> as the user type and <see cref="IdentityRole"/> as the role type.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to register Identity services into.</param>
        /// <returns>The updated <see cref="IServiceCollection"/> to allow for method chaining.</returns>
        /// <example>
        /// Example usage in <c>Program.cs</c>:
        /// <code>
        /// builder.Services.AddIdentityConfig();
        /// </code>
        /// </example>
        /// <remarks>
        /// <para>
        /// This method registers:
        /// <list type="bullet">
        ///   <item><description><see cref="UserManager{TUser}"/> for managing users.</description></item>
        ///   <item><description><see cref="RoleManager{TRole}"/> for managing roles.</description></item>
        ///   <item><description>Default token providers for features like password reset and email confirmation.</description></item>
        ///   <item><description><see cref="EntityFrameworkStoreExtensions.AddEntityFrameworkStores{TContext}(IdentityBuilder)"/> for persistence using <see cref="AppDbContext"/>.</description></item>
        /// </list>
        /// </para>
        /// <para>
        /// Password policies, lockout settings, and cookie configuration should be configured separately
        /// using <c>services.Configure&lt;IdentityOptions&gt;()</c> if required.
        /// </para>
        /// </remarks>
        public static IServiceCollection AddIdentityConfig(this IServiceCollection services)
        {
            // Configures Identity with default token providers and Entity Framework stores
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>();

            return services;
        }
    }
}