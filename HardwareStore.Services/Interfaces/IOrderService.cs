using HardwareStore.Core.Models;

namespace HardwareStore.Services.Interfaces
{
    public interface IOrderService
    {
        Task<Order> GetOrderByIDAsync(int id);
        Task<IEnumerable<Order>> GetAllOrdersAsync();
        Task CreateOrderAsync(Order order);
        Task UpdateOrderStatusAsync(int orderId, string status);
    }
}