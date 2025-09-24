using HardwareStore.Core.Enums;
using HardwareStore.Core.Models;
using HardwareStore.WebClient.ViewModels.Orders;
using HardwareStore.WebClient.ViewModels.Orders.Details;
using HardwareStore.WebClient.ViewModels.Orders.Manage;

namespace HardwareStore.WebClient.Services
{
    public interface IOrderService
    {
        IQueryable<Order> GetOrdersQuery();
        Task<int> GetTodaysOrders();
        Task<decimal> GetTotalRevenue();
        Task<int> GetPendingOrdersCount();
        Task<List<ActivityLog>> GetRecentActivities(int take = 5);
        Task<List<OrderManageVM>> GetOrdersAsync(
            string? statusFilter = null,
            DateTime? dateFilter = null);
        Task<List<OrderViewModel>> GetUserOrdersAsync(string userId, OrderStatus? statusFilter = null);
        Task<OrderDetailsViewModel?> GetOrderDetailsAsync(int orderId, string userId);
        Task UpdateOrderStatusAsync(int orderId, OrderStatus status);
        Task CancelOrderAsync(int orderId);
    }
}