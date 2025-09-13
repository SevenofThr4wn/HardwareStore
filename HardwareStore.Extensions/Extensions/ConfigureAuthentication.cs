using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

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
            }).AddCookie()
              .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
              {
                  var oidc = configuration.GetSection("Keycloak");

                  options.Authority = oidc["Authority"];
                  options.ClientId = oidc["ClientId"];
                  options.ClientSecret = oidc["ClientSecret"];
                  options.CallbackPath = oidc["CallbackPath"];
                  options.ResponseType = oidc["ResponseType"];

                  options.SaveTokens = true;
                  options.RequireHttpsMetadata = false;
                  options.GetClaimsFromUserInfoEndpoint = true;

                  options.TokenValidationParameters = new TokenValidationParameters
                  {
                      NameClaimType = "preferred_username",
                      RoleClaimType = "roles"
                  };
              });
            return services;
        }

    }
}
