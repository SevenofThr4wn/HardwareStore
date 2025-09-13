using Microsoft.AspNetCore.Identity;

namespace HardwareStore.Data.Identity
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
    }
}
