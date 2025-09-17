using HardwareStore.WebClient.Models;

namespace HardwareStore.WebClient.ViewModels.Account
{
    public class ManagerDashboardViewModel
    {
        public int TotalProducts { get; set; }
        public int LowStockProducts { get; set; }
        public int PendingOrders { get; set; }
        public int TotalStaff { get; set; }
        public List<ProductStockInfo> LowStockItems { get; set; } = new();
    }
}
