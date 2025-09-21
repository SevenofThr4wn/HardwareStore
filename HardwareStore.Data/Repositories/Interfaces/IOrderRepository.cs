using HardwareStore.Core.Enums;
using HardwareStore.Core.Models;

namespace HardwareStore.Data.Repositories.Interfaces
{
    public interface IOrderRepository
    {
        Task<Order> GetByIdAsync(int id);
        Task<IEnumerable<Order>> GetAllAsync();
        Task AddAsync(Order order);
        Task UpdateAsync(Order order);
        Task<int> DeleteAsync(int id);

        Task UpdateOrderStatusAsync(int orderId, OrderStatus status);
    }
}
