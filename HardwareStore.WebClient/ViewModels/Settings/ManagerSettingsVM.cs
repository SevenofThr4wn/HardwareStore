namespace HardwareStore.WebClient.ViewModels.Settings
{
    public class ManagerSettingsVM
    {
        public bool LowStockNotifications { get; set; }
        public bool CriticalStockNotifications { get; set; }
        public int LowStockThreshold { get; set; }
        public bool AutoConfirmOrders { get; set; }
        public bool RequireManagerApproval { get; set; }
    }
}
