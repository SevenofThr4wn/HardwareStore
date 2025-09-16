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
        public static IServiceCollection ConfigureKeycloakAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var authority = configuration["Keycloak:Authority"];
            var clientId = configuration["Keycloak:ClientId"];

            if (string.IsNullOrEmpty(authority))
                throw new InvalidOperationException("Keycloak:Authority must be configured.");

            if (!authority.EndsWith("/")) authority += "/";

            var wellKnownUrl = new Uri(new Uri(authority), ".well-known/openid-configuration").AbsoluteUri;

            var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                wellKnownUrl,
                new OpenIdConnectConfigurationRetriever(),
                new HttpDocumentRetriever { RequireHttps = false }
            );

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.SlidingExpiration = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
            })
            .AddJwtBearer(options =>
            {
                options.Authority = authority;
                options.RequireHttpsMetadata = false;
                options.ConfigurationManager = configurationManager;

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
                            }
                        }

                        if (configuredAudiences.Any() && tokenAudiences.Any(a => configuredAudiences.Contains(a)))
                            return true;

                        return false;
                    }
                };

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        if (context.Principal?.Identity is ClaimsIdentity identity)
                        {
                            // Remove existing role claims
                            var existingRoles = identity.FindAll(ClaimTypes.Role).ToList();
                            foreach (var r in existingRoles) identity.RemoveClaim(r);

                            // Extract realm roles
                            AddRolesFromJsonClaim(identity, "realm_access", "roles");

                            // Extract client roles from resource_access
                            AddRolesFromJsonClaim(identity, "resource_access", null);

                            // Log roles
                            var rolesFound = identity.FindAll(ClaimTypes.Role).Select(c => c.Value).Distinct();
                            Console.WriteLine("JWT validated with roles: " + string.Join(", ", rolesFound));
                        }
                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine("JWT auth failed: " + context.Exception?.Message);
                        return Task.CompletedTask;
                    }
                };
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("RequireAuthenticatedUser", p => p.RequireAuthenticatedUser());
                options.AddPolicy("RequireAdminRole", p => p.RequireRole("Admin"));
                options.AddPolicy("RequireStaffRole", p => p.RequireRole("Staff", "Admin", "Manager"));
            });

            services.AddTransient<IClaimsTransformation, ClaimsTransformer>();

            return services;
        }

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
                else if (root.ValueKind == JsonValueKind.Array) // handle arrays like your realm_access
                {
                    foreach (var role in root.EnumerateArray())
                    {
                        var roleName = role.GetString();
                        if (!string.IsNullOrEmpty(roleName))
                            identity.AddClaim(new Claim(ClaimTypes.Role, roleName));
                    }
                }
                else if (rolesProperty == null && root.ValueKind == JsonValueKind.Object) // resource_access
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