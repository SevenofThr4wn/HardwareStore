using HardwareStore.Core.Models;
using HardwareStore.WebClient.Models;

namespace HardwareStore.WebClient.ViewModels.Account
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<QuickAction> QuickActions { get; set; } = new();
        public List<ActivityLog> RecentActivity { get; set; } = new();
    }
}
