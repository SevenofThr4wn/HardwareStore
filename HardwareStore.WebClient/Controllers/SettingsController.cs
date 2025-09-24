using HardwareStore.WebClient.ViewModels.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HardwareStore.WebClient.Controllers
{
    public class SettingsController : Controller
    {
        private readonly IConfiguration _config;

        public SettingsController(IConfiguration config)
        {
            _config = config;
        }

        [Authorize(Roles = "Admin,Manager")]
        public IActionResult AdminSettings()
        {
            var settings = new AdminSettingsVM
            {
                StoreName = _config["StoreSettings:Name"] ?? "Hardware Store",
                StoreEmail = _config["StoreSettings:Email"] ?? "admin@hardwarestore.com",
                Currency = _config["StoreSettings:Currency"] ?? "USD",
                SessionTimeout = int.Parse(_config["SecuritySettings:SessionTimeout"] ?? "60"),
                PasswordPolicy = _config["SecuritySettings:PasswordPolicy"] ?? "Standard"
            };

            return View(settings);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult SaveAdminSettings(AdminSettingsVM model)
        {
            if (!ModelState.IsValid)
            {
                return View("AdminSettings", model);
            }

            TempData["SuccessMessage"] = "Settings saved successfully";
            return RedirectToAction("AdminSettings");
        }

        [Authorize(Roles = "Admin,Manager")]
        public IActionResult ManagerSettings()
        {
            var settings = new ManagerSettingsVM
            {
                LowStockNotifications = bool.Parse(_config["InventorySettings:LowStockNotifications"] ?? "true"),
                CriticalStockNotifications = bool.Parse(_config["InventorySettings:CriticalStockNotifications"] ?? "true"),
                LowStockThreshold = int.Parse(_config["InventorySettings:LowStockThreshold"] ?? "10"),
                AutoConfirmOrders = bool.Parse(_config["OrderSettings:AutoConfirmOrders"] ?? "true"),
                RequireManagerApproval = bool.Parse(_config["OrderSettings:RequireManagerApproval"] ?? "false")
            };

            return View(settings);
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        public IActionResult SaveManagerSettings(ManagerSettingsVM model)
        {
            if (!ModelState.IsValid)
            {
                return View("ManagerSettings", model);
            }

            // Save settings logic here

            TempData["SuccessMessage"] = "Preferences saved successfully";
            return RedirectToAction("ManagerSettings");
        }
    }
}