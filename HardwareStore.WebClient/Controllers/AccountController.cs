using HardwareStore.Data.Helper;
using HardwareStore.Data.Identity;
using HardwareStore.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.CompilerServices;

namespace HardwareStore.WebClient.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        private readonly KeycloakHelper _keycloakHelper;
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountController(
            IUserService userService,
            KeycloakHelper keycloakHelper,
            UserManager<ApplicationUser> userManager)
        {
            _userService = userService;
            _keycloakHelper = keycloakHelper;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(string username, string email, string password)
        {

            // 1. Create local EF user
            var user = new HardwareStore.Core.Models.User
            {
                Username = username,
                Email = email
            };
            await _userService.CreateUserAsync(user, password);

            // 2. Map to Keycloak user
            var kcUser = new Keycloak.Net.Models.Users.User
            {
                UserName = user.Username,
                Email = user.Email,
                Enabled = true
            };

            // 3. Call Keycloak helper
            await _keycloakHelper.CreateUserAsync(kcUser, password);

            // 4. Add to ASP.NET Identity
            var identityUser = new ApplicationUser { UserName = username, Email = email };
            await _userManager.CreateAsync(identityUser, password);

            return RedirectToAction("Login");
        }

        [HttpGet("login")]
        public IActionResult Login(string? returnUrl = "/")
        {
            return Challenge(new AuthenticationProperties { RedirectUri = returnUrl ?? "/" },
                OpenIdConnectDefaults.AuthenticationScheme);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            var idToken = await HttpContext.GetTokenAsync("id_token");
            var clientId = _keycloakHelper.GetClientId();
            var postLogoutRedirect = Url.Action("Index", "Home", null, Request.Scheme) ?? "/";

            string logoutUrl;

            if (!string.IsNullOrEmpty(idToken))
            {
                // Preferred way: use id_token_hint
                logoutUrl = $"{_keycloakHelper.GetKeycloakAuthority()}/protocol/openid-connect/logout?" +
                            $"id_token_hint={Uri.EscapeDataString(idToken)}&" +
                            $"post_logout_redirect_uri={Uri.EscapeDataString(postLogoutRedirect)}";
            }
            else
            {
                // Fallback: use client_id if id_token is missing
                logoutUrl = $"{_keycloakHelper.GetKeycloakAuthority()}/protocol/openid-connect/logout?" +
                            $"client_id={Uri.EscapeDataString(clientId)}&" +
                            $"post_logout_redirect_uri={Uri.EscapeDataString(postLogoutRedirect)}";
            }

            // Clear local auth cookies
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return Redirect(logoutUrl);
        }

        
    }
}
