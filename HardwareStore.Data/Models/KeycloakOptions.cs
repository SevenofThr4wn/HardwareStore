namespace HardwareStore.Data.Models
{
    public class KeycloakOptions
    {
        public string ServerUrl { get; set; } = string.Empty;
        public string Realm { get; set; } = string.Empty;
        public string AdminUser { get; set; } = string.Empty;
        public string AdminPassword { get; set; } = string.Empty;
    }
}
