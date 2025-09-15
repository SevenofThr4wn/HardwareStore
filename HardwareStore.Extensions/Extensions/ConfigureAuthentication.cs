using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace HardwareStore.Extensions.Extensions
{
    public static class ConfigureAuthentication
    {
        public static IServiceCollection ConfigureKeycloakAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.None;
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
                options.SlidingExpiration = true;

                options.Events = new CookieAuthenticationEvents
                {
                    OnSigningIn = context =>
                    {
                        Console.WriteLine("Cookie signing in process started");
                        return Task.CompletedTask;
                    },
                    OnSignedIn = context =>
                    {
                        Console.WriteLine("Cookie signed in successfully");
                        Console.WriteLine($"User authenticated: {context.Principal?.Identity?.IsAuthenticated}");
                        return Task.CompletedTask;
                    },
                    OnValidatePrincipal = context =>
                    {
                        Console.WriteLine("Validating cookie principal");
                        return Task.CompletedTask;
                    }
                };
            })
            .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
            {
                var oidc = configuration.GetSection("Keycloak");

                options.Authority = oidc["Authority"];
                options.ClientId = oidc["ClientId"];
                options.ResponseType = oidc["ResponseType"]!;
                options.CallbackPath = oidc["CallbackPath"];


                options.ClaimActions.Clear();
                options.ClaimActions.MapJsonKey(ClaimTypes.Role, "roles"); 
                options.ClaimActions.MapJsonKey("roles", "roles"); 

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = "preferred_username",
                    RoleClaimType = ClaimTypes.Role,
                    ValidateIssuer = true
                };

                options.RequireHttpsMetadata = false;

                options.SaveTokens = true;
                options.GetClaimsFromUserInfoEndpoint = true;

                options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;

                // Explicitly set the sign-out paths
                options.SignedOutCallbackPath = "/signout-callback-oidc";
                options.RemoteSignOutPath = "/signout-oidc";

                // SameSite settings
                options.CorrelationCookie.SameSite = SameSiteMode.None;
                options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;

                // Scopes
                options.Scope.Clear();
                options.Scope.Add("openid");
                options.Scope.Add("profile");
                options.Scope.Add("email");

                // Configures Event Handling & Logging
                options.Events = new OpenIdConnectEvents
                {
                    OnRedirectToIdentityProvider = context =>
                    {
                        Console.WriteLine($"Redirecting to: {context.ProtocolMessage.IssuerAddress}");
                        return Task.CompletedTask;
                    },
                    OnTicketReceived = context =>
                    {
                        Console.WriteLine("Authentication ticket received");
                        Console.WriteLine($"IsAuthenticated: {context.Principal?.Identity?.IsAuthenticated}");
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        Console.WriteLine("Token validated - checking principal");
                        Console.WriteLine($"Principal name: {context.Principal?.Identity?.Name}");
                        Console.WriteLine($"IsAuthenticated before: {context.Principal?.Identity?.IsAuthenticated}");

                        if (context.Principal?.Identity is ClaimsIdentity identity &&
                            !identity.IsAuthenticated &&
                            identity.Claims.Any())
                        {
                            Console.WriteLine("Fixing unauthenticated identity...");
                            var newIdentity = new ClaimsIdentity(
                                identity.Claims,
                                CookieAuthenticationDefaults.AuthenticationScheme,
                                identity.NameClaimType,
                                identity.RoleClaimType);

                            context.Principal = new ClaimsPrincipal(newIdentity);
                            Console.WriteLine($"IsAuthenticated after: {context.Principal.Identity!.IsAuthenticated}");
                        }

                        return Task.CompletedTask;
                    },
                    OnUserInformationReceived = context =>
                    {
                        Console.WriteLine("User information received");
                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine($"Authentication failed: {context.Exception?.Message}");
                        Console.WriteLine($"Exception: {context.Exception}");
                        return Task.CompletedTask;
                    },
                    OnRemoteFailure = context =>
                    {
                        Console.WriteLine($"Remote failure: {context.Failure?.Message}");
                        Console.WriteLine($"Error: {context.Failure}");
                        return Task.CompletedTask;
                    },
                    OnMessageReceived = context =>
                    {
                        Console.WriteLine($"Message received: {context.ProtocolMessage?.Error}");
                        return Task.CompletedTask;
                    }
                };
            });

            services.AddTransient<IClaimsTransformation, ClaimsTransformer>();

            return services;
        }

        public static IServiceCollection ConfigureCookies(this IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
                options.OnAppendCookie = cookieContext =>
                    CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
                options.OnDeleteCookie = cookieContext =>
                    CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
            });
            return services;
        }

        private static void CheckSameSite(HttpContext httpContext, CookieOptions options)
        {
            if (options.SameSite == SameSiteMode.None)
            {
                var userAgent = httpContext.Request.Headers["User-Agent"].ToString();
            }
        }
    }

    public class ClaimsTransformer : IClaimsTransformation
    {
        public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            Console.WriteLine($"ClaimsTransformer - Input IsAuthenticated: {principal?.Identity?.IsAuthenticated}");

            if (principal?.Identity is ClaimsIdentity identity &&
                !identity.IsAuthenticated &&
                identity.Claims.Any())
            {
                Console.WriteLine("Transforming unauthenticated identity to authenticated");

                var newIdentity = new ClaimsIdentity(
                    identity.Claims,
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    identity.NameClaimType,
                    identity.RoleClaimType);

                var newPrincipal = new ClaimsPrincipal(newIdentity);
                Console.WriteLine($"ClaimsTransformer - Output IsAuthenticated: {newPrincipal.Identity!.IsAuthenticated}");

                return Task.FromResult(newPrincipal);
            }

            return Task.FromResult(principal)!;
        }
    }
}