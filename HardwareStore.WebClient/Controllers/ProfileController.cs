using HardwareStore.WebClient.ViewModels.Profile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HardwareStore.WebClient.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        // Page 1: Profile Overview - /Profile

        [HttpGet]
        public IActionResult Index()
        {
            var userProfile = new ProfileOverviewViewModel
            {
                DisplayName = User.Identity!.Name!,
                Email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("email")!,
                FullName = $"{User.FindFirstValue(ClaimTypes.GivenName) ?? User.FindFirstValue("given_name")} " +
                $"{User.FindFirstValue(ClaimTypes.Surname) ?? User.FindFirstValue("family_name")}",
            };
            return View(userProfile);
        }


        // Page 2: Security Settings - /Profile/SecuritySettings

        [HttpGet("/SecuritySettings")]
        public IActionResult Security()
        {


            return View();
        }

        // Page 3: Personal Information - /Profile/PersonalInfo

        [HttpGet("/PersonalInfo")]
        public IActionResult PersonalInfo()
        {
            return View();
        }

        // Page 3: Payment & Shipping - /Profile/PaymentShipping

        [HttpGet("/PaymentShipping")]
        public IActionResult PaymentShipping()
        {
            return View();
        }

        // Page 4: Orders - /Profile/Orders

        [HttpGet("/Orders")]
        public IActionResult Orders()
        {
            return View();
        }



        [HttpPost]
        public IActionResult ChangePassword()
        {
            return View();
        }
    }
}