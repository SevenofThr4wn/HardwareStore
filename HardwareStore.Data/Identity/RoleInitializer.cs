using Microsoft.AspNetCore.Identity;

namespace HardwareStore.Data.Identity
{
    public static class RoleInitializer
    {
        public static async Task InitAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            string adminRole = "Admin";

            // If the admin role does not exist in the database, create it
            if (!await roleManager.RoleExistsAsync(adminRole))
            {
                await roleManager.CreateAsync(new IdentityRole(adminRole));
            }

            string adminEmail = "admin@hardwarestore.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            // if the admin user does not exist, create it and assign the admin role
            if (adminUser == null)
            {
                adminUser = new ApplicationUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
                await userManager.CreateAsync(adminUser, "Admin@1234");
                await userManager.AddToRoleAsync(adminUser, adminRole);
            }
        }
    }
}