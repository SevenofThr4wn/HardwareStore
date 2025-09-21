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
    public static class ConfigureAuthentication
    {
        /// <summary>
        /// Configures authentication and authorization for Keycloak integration.
        /// Sets up cookie and JWT bearer authentication, maps Keycloak claims to .NET claims,
        /// and registers authorization policies and claims transformation.
        /// </summary>
        /// <param name="services">The service collection to add authentication services to.</param>
        /// <param name="configuration">The application configuration containing Keycloak settings.</param>
        /// <returns>The updated service collection.</returns>
        /// <exception cref="InvalidOperationException">Thrown if Keycloak:Authority is not configured.</exception>
        public static IServiceCollection ConfigureKeycloakAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            // Reads the Keycloak server URL and client id from appsettings.json
            var authority = configuration["Keycloak:Authority"];
            var clientId = configuration["Keycloak:ClientId"];

            // throws an error if Authority is missing in the json file.
            if (string.IsNullOrEmpty(authority))
                throw new InvalidOperationException("Keycloak:Authority must be configured. Please see appsettings.json for more information");

            // Ensures the authority URL ends with a / for proper URI concatentation.
            if (!authority.EndsWith("/")) authority += "/";

            // Builds the well-known OpenID Connect configuration URL
            // Example: https://keycloak.example.com/auth/realms/myrealm/.wel-known/openid-configuration
            var wellKnownUrl = new Uri(new Uri(authority), ".well-known/openid-configuration").AbsoluteUri;

            // Creates a ConfigurationManager that fetches Keycloak's OpenID configuration dynamically.
            // RequireHttps = false allows non-https connections
            var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                wellKnownUrl,
                new OpenIdConnectConfigurationRetriever(),
                new HttpDocumentRetriever { RequireHttps = false }
            );

            // Registers authentication schemes
            //      - Default scheme: cookie-based authentication
            //      - Default challenge: JWT Bearer(used when the user needs to login).
            //      - Default sign-in & authentication cookies
            services.AddAuthentication(options =>
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
            .AddCookie(options =>
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
            .AddJwtBearer(options =>
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
                    AudienceValidator = (IEnumerable<string> auds, SecurityToken securityToken, TokenValidationParameters tvp) =>
                    {
                        var jwt = securityToken as JwtSecurityToken;
                        if (jwt == null) return false;

                        var tokenAudiences = jwt.Audiences?.ToList() ?? new List<string>();
                        var configuredAudiences = auds?.ToList() ?? new List<string>();

                        if (tokenAudiences.Contains(clientId!)) return true;

                        var azp = jwt.Claims.FirstOrDefault(c => c.Type == "azp")?.Value;
                        if (!string.IsNullOrEmpty(azp) && azp == clientId) return true;

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

                        if (configuredAudiences.Any() && tokenAudiences.Any(a => configuredAudiences.Contains(a)))
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
                    OnTokenValidated = context =>
                    {
                        if (context.Principal?.Identity is ClaimsIdentity identity)
                        {
                            // Remove existing role claims
                            var existingRoles = identity.FindAll(ClaimTypes.Role).ToList();
                            foreach (var r in existingRoles) identity.RemoveClaim(r);

                            AddRolesFromJsonClaim(identity, "realm_access", "roles");

                            AddRolesFromJsonClaim(identity, "resource_access", null);

                            // If sucessfully validates all the user claims, print message to the console
                            var rolesFound = identity.FindAll(ClaimTypes.Role).Select(c => c.Value).Distinct();
                            Console.WriteLine("JWT successfully validated with roles: " + string.Join(", ", rolesFound));
                        }
                        return Task.CompletedTask;
                    },

                    // If the user authentication fails,
                    // then a message is printed to the console window with the exception message.
                    OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine("JWT authentication failed: " + context.Exception?.Message);
                        return Task.CompletedTask;
                    }
                };
            });

            // Registers authorization policies
            //  - Require authenticated user.
            //  - Require specific role(Admin, Staff, etc.).
            services.AddAuthorization(options =>
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
        /// Adds role claims from a JSON claim in the JWT to the provided ClaimsIdentity.
        /// Handles both "realm_access" and "resource_access" claim structures from Keycloak.
        /// </summary>
        /// <param name="identity">The ClaimsIdentity to add roles to.</param>
        /// <param name="claimType">The claim type to extract roles from (e.g., "realm_access" or "resource_access").</param>
        /// <param name="rolesProperty">The property name within the claim that contains the roles array, or null for "resource_access".</param>
        private static void AddRolesFromJsonClaim(ClaimsIdentity identity, string claimType, string? rolesProperty)
        {
            var claim = identity.FindFirst(claimType);
            if (claim == null) return;

            try
            {
                using var doc = JsonDocument.Parse(claim.Value);
                var root = doc.RootElement;

                if (root.ValueKind == JsonValueKind.Object && rolesProperty != null && root.TryGetProperty(rolesProperty, out var roles) && roles.ValueKind == JsonValueKind.Array)
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
                        if (client.Value.TryGetProperty("roles", out var clientRoles) && clientRoles.ValueKind == JsonValueKind.Array)
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

        public class ClaimsTransformer : IClaimsTransformation
        {
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