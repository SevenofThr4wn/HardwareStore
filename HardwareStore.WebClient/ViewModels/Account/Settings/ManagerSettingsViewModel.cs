namespace HardwareStore.WebClient.ViewModels.Account.Settings
{
    public class ManagerSettingsViewModel
    {
        public bool LowStockNotifications { get; set; }
        public bool CriticalStockNotifications { get; set; }
        public int LowStockThreshold { get; set; }
        public bool AutoConfirmOrders { get; set; }
        public bool RequireManagerApproval { get; set; }
    }
}
