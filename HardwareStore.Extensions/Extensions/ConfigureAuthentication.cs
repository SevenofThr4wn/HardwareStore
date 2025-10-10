using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;

namespace HardwareStore.Extensions.Extensions
{
    /// <summary>
    /// Provides extension methods to configure authentication and authorization with Keycloak integration in an ASP.NET
    /// Core application.
    /// </summary>
    /// <remarks>
    /// This class configures both cookie and JWT bearer authentication, integrates Keycloak as the identity provider,
    /// sets up token validation, and registers authorization policies. It also provides helper methods to extract role
    /// claims from Keycloak JWTs and normalizes claims for cookie-based authentication scenarios.
    /// </remarks>
    public static class ConfigureAuthentication
    {
        /// <summary>
        /// Configures Keycloak-based authentication and authorization.
        /// </summary>
        /// <param name="services">The service collection to register authentication and authorization services into.</param>
        /// <param name="configuration">
        /// The application configuration, expected to contain Keycloak settings such as <c>Keycloak:Authority</c> and
        /// <c>Keycloak:ClientId</c>.
        /// </param>
        /// <returns>The updated <see cref="IServiceCollection"/> with Keycloak authentication and authorization configured.</returns>
        /// <exception cref="InvalidOperationException">Thrown if <c>Keycloak:Authority</c> is missing or invalid in configuration.</exception>
        /// <remarks>
        /// This method: <list type="bullet"><item><description>Registers cookie authentication as the default
        /// scheme.</description></item><item><description>Configures JWT bearer authentication against a Keycloak
        /// authority.</description></item><item><description>Defines custom audience validation logic to support
        /// Keycloak-specific tokens.</description></item><item><description>Maps Keycloak claims
        /// (<c>preferred_username</c>, <c>realm_access</c>, <c>resource_access</c>) into .NET
        /// claims.</description></item><item><description>Sets up authorization policies for authenticated users and
        /// role-based access control.</description></item><item><description>Registers a custom claims transformer to
        /// normalize identities to the cookie authentication scheme.</description></item></list>
        /// </remarks>
        public static IServiceCollection ConfigureKeycloakAuthentication(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Reads the Keycloak server URL and client id from appsettings.json
            var authority = configuration["Keycloak:Authority"];
            var clientId = configuration["Keycloak:ClientId"];

            if (string.IsNullOrEmpty(authority))
                throw new InvalidOperationException(
                    "Keycloak:Authority must be configured. Please see appsettings.json for more information");

            if (!authority.EndsWith("/"))
                authority += "/";

            var wellKnownUrl = new Uri(new Uri(authority), ".well-known/openid-configuration").AbsoluteUri;

            var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                wellKnownUrl,
                new OpenIdConnectConfigurationRetriever(),
                new HttpDocumentRetriever { RequireHttps = false });

            // Registers authentication schemes
            //      - Default scheme: cookie-based authentication
            //      - Default challenge: JWT Bearer(used when the user needs to login).
            //      - Default sign-in & authentication cookies
            services.AddAuthentication(
                options =>
                {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                })
                // Configures Cookie authentication'
                //      - Login and Access Denied paths
                //      - Sliding expiration => extends the cookie lifetime when the user is active.
                //      - Cookie expires after 120 minutes.
                .AddCookie(
                    options =>
                    {
                        options.LoginPath = "/Account/Login";
                        options.AccessDeniedPath = "/Account/AccessDenied";
                        options.SlidingExpiration = true;
                        options.ExpireTimeSpan = TimeSpan.FromMinutes(120);
                    })

            // Configures JWT Bearer authentication
            //      - Authority => Keycloak Server
            //      - RequireHttpsMetadata = false => allows HTTP.
            //      - ConfigurationManager => uses the OpenID configuration fetched earlier
            .AddJwtBearer(
                options =>
                {
                    options.Authority = authority;
                    options.RequireHttpsMetadata = false;
                    options.ConfigurationManager = configurationManager;


                    // JWT validation paramters
                    //      - Validates issuer, audience, and token lifetime.
                    //      - Maps Keycloak claims to .NET claims
                    //          - preffered_username => Name
                    //          - ClaimTypes.Role => Roles
                    //      - Custom AudienceValidator => checks multiple claims in Keycloak JWTs. 
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = authority.TrimEnd('/'),
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        NameClaimType = "preferred_username",
                        RoleClaimType = ClaimTypes.Role,
                        AudienceValidator =
                            (auds, securityToken, tvp) =>
                            {
                                if (securityToken is not JwtSecurityToken jwt)
                                    return false;

                                var tokenAudiences = jwt.Audiences?.ToList() ?? [];
                                var configuredAudiences = auds?.ToList() ?? [];

                                if (tokenAudiences.Contains(clientId!))
                                    return true;

                                var azp = jwt.Claims.FirstOrDefault(c => c.Type == "azp")?.Value;
                                if (!string.IsNullOrEmpty(azp) && azp == clientId)
                                    return true;

                                var resourceAccess = jwt.Claims.FirstOrDefault(c => c.Type == "resource_access")?.Value;
                                if (!string.IsNullOrEmpty(resourceAccess))
                                {
                                    try
                                    {
                                        using var doc = JsonDocument.Parse(resourceAccess);
                                        if (doc.RootElement.ValueKind == JsonValueKind.Object &&
                                            doc.RootElement.TryGetProperty(clientId!, out _))
                                        {
                                            return true;
                                        }
                                    }
                                    catch
                                    {
                                        Console.WriteLine("Error whilst reading JSON document");
                                    }
                                }

                                if (configuredAudiences.Count != 0 &&
                                    tokenAudiences.Any(a => configuredAudiences.Contains(a)))
                                    return true;

                                return false;
                            }
                    };

                    // JWT events
                    //      - OnTokenValidated => triggered after sucessful token validation"
                    //          - Clears old roles
                    //          - Add roles from Keycloak JSON claims.
                    //          - Logs roles to console.
                    //      - OnAuthenticationFailed => triggered if JWT validation fails; log the exception.
                    options.Events = new JwtBearerEvents
                    {
                        OnTokenValidated =
                            context =>
                            {
                                if (context.Principal?.Identity is ClaimsIdentity identity)
                                {
                                    // Remove existing role claims
                                    var existingRoles = identity.FindAll(ClaimTypes.Role).ToList();
                                    foreach (var r in existingRoles)
                                        identity.RemoveClaim(r);

                                    AddRolesFromJsonClaim(identity, "realm_access", "roles");

                                    AddRolesFromJsonClaim(identity, "resource_access", null);

                                    // If sucessfully validates all the user claims, print message to the console
                                    var rolesFound = identity.FindAll(ClaimTypes.Role).Select(c => c.Value).Distinct();
                                    Console.WriteLine(
                                        "JWT successfully validated with roles: " + string.Join(", ", rolesFound));
                                }
                                return Task.CompletedTask;
                            },

                        // If the user authentication fails,
                        // then a message is printed to the console window with the exception message.
                        OnAuthenticationFailed =
                            context =>
                            {
                                Console.WriteLine("JWT authentication failed: " + context.Exception?.Message);
                                return Task.CompletedTask;
                            }
                    };
                });

            // Registers authorization policies
            //  - Require authenticated user.
            //  - Require specific role(Admin, Staff, etc.).
            services.AddAuthorization(
                options =>
                {
                    options.AddPolicy("RequireAuthenticatedUser", p => p.RequireAuthenticatedUser());
                    options.AddPolicy("RequireAdminRole", p => p.RequireRole("Admin"));
                    options.AddPolicy("RequireStaffRole", p => p.RequireRole("Staff", "Admin", "Manager"));
                });

            // Registers a custom roles transformer to map JWT claims to cookie claims.
            services.AddTransient<IClaimsTransformation, ClaimsTransformer>();

            return services;
        }

        /// <summary>
        /// Adds role claims to a <see cref="ClaimsIdentity"/> from JSON claims within a Keycloak JWT.
        /// </summary>
        /// <param name="identity">The <see cref="ClaimsIdentity"/> to which role claims will be added.</param>
        /// <param name="claimType">
        /// The claim type in the JWT that contains role information. Typically <c>realm_access</c> for global roles or
        /// <c>resource_access</c> for client-specific roles.
        /// </param>
        /// <param name="rolesProperty">
        /// The property name inside the claim that contains the roles array. Pass <c>null</c> for
        /// <c>resource_access</c> claims, since they have nested role structures.
        /// </param>
        /// <remarks>
        /// This method safely parses Keycloak’s role structures and adds them as <see cref="ClaimTypes.Role"/> claims
        /// to the identity. It handles: <list type="bullet"><item><description><c>realm_access.roles</c> JSON
        /// arrays.</description></item><item><description><c>resource_access.{client}.roles</c> JSON
        /// arrays.</description></item><item><description>Flat role arrays directly in a
        /// claim.</description></item></list> If parsing fails, an error is logged to the console but execution
        /// continues without throwing.
        /// </remarks>
        private static void AddRolesFromJsonClaim(ClaimsIdentity identity, string claimType, string? rolesProperty)
        {
            var claim = identity.FindFirst(claimType);
            if (claim == null)
                return;

            try
            {
                using var doc = JsonDocument.Parse(claim.Value);
                var root = doc.RootElement;

                if (root.ValueKind == JsonValueKind.Object &&
                    rolesProperty != null &&
                    root.TryGetProperty(rolesProperty, out var roles) &&
                    roles.ValueKind == JsonValueKind.Array)
                {
                    foreach (var role in roles.EnumerateArray())
                    {
                        var roleName = role.GetString();
                        if (!string.IsNullOrEmpty(roleName))
                            identity.AddClaim(new Claim(ClaimTypes.Role, roleName));
                    }
                }
                else if (root.ValueKind == JsonValueKind.Array)
                {
                    foreach (var role in root.EnumerateArray())
                    {
                        var roleName = role.GetString();
                        if (!string.IsNullOrEmpty(roleName))
                            identity.AddClaim(new Claim(ClaimTypes.Role, roleName));
                    }
                }
                else if (rolesProperty == null && root.ValueKind == JsonValueKind.Object)
                {
                    foreach (var client in root.EnumerateObject())
                    {
                        if (client.Value.TryGetProperty("roles", out var clientRoles) &&
                            clientRoles.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var role in clientRoles.EnumerateArray())
                            {
                                var roleName = role.GetString();
                                if (!string.IsNullOrEmpty(roleName))
                                    identity.AddClaim(new Claim(ClaimTypes.Role, roleName));
                            }
                        }
                    }
                }
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Failed to parse {claimType}: {ex.Message}");
            }
        }


        /// <summary>
        /// A claims transformer that ensures authenticated users have their identity represented with the cookie
        /// authentication scheme.
        /// </summary>
        /// <remarks>
        /// This is useful when a user may authenticate using a different scheme (e.g., JWT, OAuth), but the application
        /// expects claims to be associated with <see cref="CookieAuthenticationDefaults.AuthenticationScheme"/> for
        /// consistency.
        /// </remarks>
        public class ClaimsTransformer : IClaimsTransformation
        {
            /// <summary>
            /// Transforms the incoming <see cref="ClaimsPrincipal"/> to use the cookie authentication scheme.
            /// </summary>
            /// <param name="principal">The original <see cref="ClaimsPrincipal"/> from authentication middleware.</param>
            /// <returns>
            /// A <see cref="ClaimsPrincipal"/> with a <see cref="ClaimsIdentity"/> based on the cookie scheme, or the
            /// original principal if no transformation is required.
            /// </returns>
            /// <remarks>
            /// <para> Transformation occurs only if the identity is authenticated but uses a scheme other than <see
            /// cref="CookieAuthenticationDefaults.AuthenticationScheme"/>.</para> <para> Claims, name claim type, and
            /// role claim type are preserved when creating the new identity.</para>
            /// </remarks>
            public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
            {
                if (principal?.Identity is ClaimsIdentity identity && identity.IsAuthenticated)
                {
                    if (identity.AuthenticationType != CookieAuthenticationDefaults.AuthenticationScheme)
                    {
                        var newIdentity = new ClaimsIdentity(
                            identity.Claims,
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            identity.NameClaimType,
                            identity.RoleClaimType);
                        return Task.FromResult(new ClaimsPrincipal(newIdentity));
                    }
                }
                return Task.FromResult(principal!);
            }
        }
    }
}