namespace HardwareStore.WebClient.ViewModels.Account.Settings
{
    public class AdminSettingsViewModel
    {
        public string StoreName { get; set; } = string.Empty;
        public string StoreEmail { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;
        public int SessionTimeout { get; set; }
        public string PasswordPolicy { get; set; } = string.Empty;
    }
}
