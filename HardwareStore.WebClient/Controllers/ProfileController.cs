using HardwareStore.Data.Models.Interfaces;
using HardwareStore.WebClient.ViewModels.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HardwareStore.WebClient.Controllers
{
    public class ProfileController : Controller
    {
        private readonly IUserRepository _userRepository;

        public ProfileController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        //[Authorize]
        public IActionResult Profile()
        {
            // Displays the user profile using claims from JWT/cookie.
            var userProfile = new ProfileViewModel
            {
                DisplayName = User.Identity!.Name!,
                Email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("email")!,
                FullName = $"{User.FindFirstValue(ClaimTypes.GivenName) ?? User.FindFirstValue("given_name")} " +
                $"{User.FindFirstValue(ClaimTypes.Surname) ?? User.FindFirstValue("family_name")}",
            };
            return View(userProfile);
        }

        [Authorize]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userName = User.Identity!.Name;
            var user = await _userRepository.GetByUsernameAsync(userName!);

            if (user == null)
            {
                return NotFound();
            }

            TempData["SuccessMessage"] = "Password changed successfully";
            return RedirectToAction("Profile");
        }
    }
}