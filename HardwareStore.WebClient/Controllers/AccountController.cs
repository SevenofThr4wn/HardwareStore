using HardwareStore.WebClient.ViewModels.Account;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace HardwareStore.WebClient.Controllers
{
    public class AccountController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public AccountController(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        [AllowAnonymous]
        public IActionResult Login(string returnUrl = "/")
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = "/")
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var token = await GetKeycloakTokenAsync(model.Username, model.Password);
               
                if (token == null)
                {
                    ModelState.AddModelError(string.Empty, "Invalid username or password");
                    return View(model);
                }

                // Validate JWT and extract claims
                var claims = await ValidateJwtAndExtractClaims(token);
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(1)
                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);

                // Debug: roles
                var userRoles = claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value);
                Console.WriteLine($"User {model.Username} logged in with roles: {string.Join(", ", userRoles)}");

                return LocalRedirect(returnUrl);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Login failed: {ex.Message}");
                return View(model);
            }
        }

        [AllowAnonymous]
        public IActionResult Register() => View();

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel viewModel)
        {
            if (!ModelState.IsValid) return View(viewModel);

            try
            {
                var success = await CreateKeycloakUserAsync(viewModel);
                if (success) return RedirectToAction("Login", new { message = "Registration successful. Please login." });

                ModelState.AddModelError(string.Empty, "Registration failed");
                return View(viewModel);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Registration failed: {ex.Message}");
                return View(viewModel);
            }
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public IActionResult Profile() => View();

        private async Task<string?> GetKeycloakTokenAsync(string username, string password)
        {
            var keycloakUrl = _configuration["Keycloak:ServerUrl"];
            var realm = _configuration["Keycloak:Realm"];
            var clientId = _configuration["Keycloak:ClientId"];

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string,string>("grant_type","password"),
                new KeyValuePair<string,string>("client_id", clientId!),
                new KeyValuePair<string,string>("username", username),
                new KeyValuePair<string,string>("password", password)
            });

            var response = await _httpClient.PostAsync($"{keycloakUrl}/realms/{realm}/protocol/openid-connect/token", content);
            if (!response.IsSuccessStatusCode) return null;

            var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            return json.RootElement.GetProperty("access_token").GetString();
        }

        private async Task<bool> CreateKeycloakUserAsync(RegisterViewModel viewModel)
        {
            var token = await GetAdminTokenAsync();
            var keycloakUrl = _configuration["Keycloak:ServerUrl"];
            var realm = _configuration["Keycloak:Realm"];

            var userData = new
            {
                username = viewModel.Username,
                email = viewModel.Email,
                firstName = viewModel.FirstName,
                lastName = viewModel.LastName,
                enabled = true,
                credentials = new[]
                {
                    new { type = "password", value = viewModel.Password, temporary = false }
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(userData), Encoding.UTF8, "application/json");
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.PostAsync($"{keycloakUrl}/admin/realms/{realm}/users", content);
            return response.IsSuccessStatusCode;
        }

        private async Task<List<Claim>> ValidateJwtAndExtractClaims(string token)
        {
            var authority = _configuration["Keycloak:Authority"];
            var clientId = _configuration["Keycloak:ClientId"];
            if (string.IsNullOrEmpty(authority)) throw new InvalidOperationException("Keycloak:Authority not configured");
            if (!authority.EndsWith("/")) authority += "/";

            var wellKnownUrl = new Uri(new Uri(authority), ".well-known/openid-configuration").AbsoluteUri;
            var configManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                wellKnownUrl,
                new OpenIdConnectConfigurationRetriever(),
                new HttpDocumentRetriever { RequireHttps = false });

            var openIdConfig = await configManager.GetConfigurationAsync();
            var signingKeys = openIdConfig.SigningKeys;

            var handler = new JwtSecurityTokenHandler();

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = authority.TrimEnd('/'),
                ValidateAudience = false,
                AudienceValidator = (audiences, token, param) =>
                {
                    var jwt = token as JwtSecurityToken;
                    if (jwt == null) return false;

                    var tokenAudiences = jwt.Audiences?.ToList() ?? new List<string>();

                    if (tokenAudiences.Contains(clientId)) return true;
                    var azp = jwt.Claims.FirstOrDefault(c => c.Type == "azp")?.Value;
                    if (!string.IsNullOrEmpty(azp) && azp == clientId) return true;

                    var resourceAccess = jwt.Claims.FirstOrDefault(c => c.Type == "resource_access")?.Value;
                    if (!string.IsNullOrEmpty(resourceAccess))
                    {
                        using var doc = JsonDocument.Parse(resourceAccess);
                        if (doc.RootElement.ValueKind == JsonValueKind.Object && doc.RootElement.TryGetProperty(clientId, out _))
                            return true;
                    }

                    return false;
                },
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,   // <--- this was false before
                IssuerSigningKeys = signingKeys,
                NameClaimType = "preferred_username",
                RoleClaimType = ClaimTypes.Role
            };


            var principal = handler.ValidateToken(token, validationParameters, out var validatedToken);
            var claims = principal.Claims.ToList();

            if (principal.Identity is ClaimsIdentity identity)
            {
                // Remove existing role claims to avoid duplicates
                var existingRoles = identity.FindAll(ClaimTypes.Role).ToList();
                foreach (var r in existingRoles) identity.RemoveClaim(r);

                // Extract realm_access roles
                var realmClaim = claims.FirstOrDefault(c => c.Type == "realm_access");
                if (realmClaim != null)
                {
                    try
                    {
                        using var doc = JsonDocument.Parse(realmClaim.Value);
                        if (doc.RootElement.TryGetProperty("roles", out var roles) && roles.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var r in roles.EnumerateArray())
                            {
                                var rn = r.GetString();
                                if (!string.IsNullOrEmpty(rn)) identity.AddClaim(new Claim(ClaimTypes.Role, rn));
                            }
                        }
                    }
                    catch { }
                }

                // Extract resource_access roles
                var resourceClaim = claims.FirstOrDefault(c => c.Type == "resource_access");
                if (resourceClaim != null)
                {
                    try
                    {
                        using var doc = JsonDocument.Parse(resourceClaim.Value);
                        if (doc.RootElement.ValueKind == JsonValueKind.Object)
                        {
                            foreach (var client in doc.RootElement.EnumerateObject())
                            {
                                if (client.Value.TryGetProperty("roles", out var clientRoles) && clientRoles.ValueKind == JsonValueKind.Array)
                                {
                                    foreach (var r in clientRoles.EnumerateArray())
                                    {
                                        var rn = r.GetString();
                                        if (!string.IsNullOrEmpty(rn)) identity.AddClaim(new Claim(ClaimTypes.Role, rn));
                                    }
                                }
                            }
                        }
                    }
                    catch { }
                }
            }

            return claims;
        }

        private async Task<string> GetAdminTokenAsync()
        {
            var keycloakUrl = _configuration["Keycloak:ServerUrl"];
            var adminUser = _configuration["Keycloak:AdminUser"];
            var adminPassword = _configuration["Keycloak:AdminPassword"];

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string,string>("grant_type","password"),
                new KeyValuePair<string,string>("client_id","admin-cli"),
                new KeyValuePair<string,string>("username", adminUser!),
                new KeyValuePair<string,string>("password", adminPassword!)
            });

            var response = await _httpClient.PostAsync($"{keycloakUrl}/realms/master/protocol/openid-connect/token", content);
            response.EnsureSuccessStatusCode();

            var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            return json.RootElement.GetProperty("access_token").GetString()!;
        }
    }
}
