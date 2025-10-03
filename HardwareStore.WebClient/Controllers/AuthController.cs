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

    /// <summary>
    /// Handles authentication and authorization flows for the application,
    /// integrating with Keycloak for login, registration, and JWT validation.
    /// </summary>
    public class AuthController : Controller
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;


        /// <summary>
        /// Initializes a new instance of the <see cref="AuthController"/>.
        /// </summary>
        /// <param name="config">The application configuration containing Keycloak settings.</param>
        /// <param name="httpClient">The HTTP client used to communicate with Keycloak endpoints.</param>
        public AuthController(IConfiguration config, HttpClient httpClient)
        {
            _config = config;
            _httpClient = httpClient;
        }

        /// <summary>
        /// Displays the login page.
        /// </summary>
        /// <param name="returnUrl">The URL to redirect the user to after successful login.</param>
        /// <returns>The login view.</returns>
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = "/")
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        /// <summary>
        /// Handles login form submission, validates user credentials against Keycloak,
        /// creates a claims principal, and issues an authentication cookie.
        /// </summary>
        /// <param name="model">The login view model containing username, password, and remember-me flag.</param>
        /// <param name="returnUrl">The URL to redirect the user to upon successful login.</param>
        /// <returns>
        /// A redirect to the requested resource on success, or the login view with error messages on failure.
        /// </returns>
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


                var claims = await ValidateJwtAndExtractClaims(token);
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(1)
                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);

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

        /// <summary>
        /// Displays the registration page.
        /// </summary>
        /// <returns>The registration view.</returns>
        [AllowAnonymous]
        public IActionResult Register() => View();

        /// <summary>
        /// Handles user registration by creating a new account in Keycloak.
        /// </summary>
        /// <param name="viewModel">The registration view model containing user details.</param>
        /// <returns>
        /// Redirects to the login page if registration succeeds, otherwise redisplays the registration view with errors.
        /// </returns>
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

        /// <summary>
        /// Logs the current user out by clearing their authentication cookie.
        /// </summary>
        /// <returns>A redirect to the home page.</returns>
        [Authorize]
        public async Task<IActionResult> Logout()
        {

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Displays the Access Denied page when a user attempts to access a forbidden resource.
        /// </summary>
        /// <returns>The AccessDenied view.</returns>
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        #region Authentication / Authorization Methods

        /// <summary>
        /// Validates a JWT access token against Keycloak's OpenID configuration and extracts user claims,
        /// including roles from <c>realm_access</c> and <c>resource_access</c>.
        /// </summary>
        /// <param name="token">The JWT access token to validate.</param>
        /// <returns>A list of claims extracted from the validated token.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the Keycloak authority is not configured.</exception>
        private async Task<List<Claim>> ValidateJwtAndExtractClaims(string token)
        {
            var authority = _config["Keycloak:Authority"];
            var clientId = _config["Keycloak:ClientId"];
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

                    if (tokenAudiences.Contains(clientId!)) return true;
                    var azp = jwt.Claims.FirstOrDefault(c => c.Type == "azp")?.Value;
                    if (!string.IsNullOrEmpty(azp) && azp == clientId) return true;

                    var resourceAccess = jwt.Claims.FirstOrDefault(c => c.Type == "resource_access")?.Value;
                    if (!string.IsNullOrEmpty(resourceAccess))
                    {
                        using var doc = JsonDocument.Parse(resourceAccess);
                        if (doc.RootElement.ValueKind == JsonValueKind.Object && doc.RootElement.TryGetProperty(clientId!, out _))
                            return true;
                    }

                    return false;
                },
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKeys = signingKeys,
                NameClaimType = "preferred_username",
                RoleClaimType = ClaimTypes.Role
            };


            var principal = handler.ValidateToken(token, validationParameters, out var validatedToken);
            var claims = principal.Claims.ToList();

            if (principal.Identity is ClaimsIdentity identity)
            {
                var existingRoles = identity.FindAll(ClaimTypes.Role).ToList();
                foreach (var r in existingRoles) identity.RemoveClaim(r);

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

        /// <summary>
        /// Requests a user access token from Keycloak using the Resource Owner Password Credentials (ROPC) flow.
        /// </summary>
        /// <param name="username">The username of the Keycloak user.</param>
        /// <param name="password">The user's password.</param>
        /// <returns>
        /// The access token string if the request is successful; otherwise <c>null</c>.
        /// </returns>
        private async Task<string?> GetKeycloakTokenAsync(string username, string password)
        {
            var keycloakUrl = _config["Keycloak:ServerUrl"];
            var realm = _config["Keycloak:Realm"];
            var clientId = _config["Keycloak:ClientId"];

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

        /// <summary>
        /// Requests an admin access token from Keycloak for use with administrative API calls.
        /// </summary>
        /// <returns>The admin access token string.</returns>
        private async Task<string> GetAdminTokenAsync()
        {
            var keycloakUrl = _config["Keycloak:ServerUrl"];
            var adminUser = _config["Keycloak:AdminUser"];
            var adminPassword = _config["Keycloak:AdminPassword"];

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

        /// <summary>
        /// Creates a new user in Keycloak using the admin API.
        /// </summary>
        /// <param name="viewModel">The registration view model containing user details.</param>
        /// <returns><c>true</c> if the user was successfully created; otherwise <c>false</c>.</returns>
        private async Task<bool> CreateKeycloakUserAsync(RegisterViewModel viewModel)
        {
            var token = await GetAdminTokenAsync();
            var keycloakUrl = _config["Keycloak:ServerUrl"];
            var realm = _config["Keycloak:Realm"];

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

        #endregion
    }
}