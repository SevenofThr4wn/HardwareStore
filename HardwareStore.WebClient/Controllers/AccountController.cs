using HardwareStore.Data.Helper;
using HardwareStore.Data.Identity;
using HardwareStore.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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
    }
}
