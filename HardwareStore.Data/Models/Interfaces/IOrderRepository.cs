using HardwareStore.Core.Enums;
using HardwareStore.Core.Models;

namespace HardwareStore.Data.Models.Interfaces
{
    public interface IOrderRepository
    {
        Task<IEnumerable<Order>> GetAllAsync();
        Task<Order> GetByIdAsync(int id);
        Task AddAsync(Order order);
        Task UpdateAsync(Order order);
        Task UpdateOrderStatusAsync(int id, OrderStatus updatedStatus);
        Task<int> DeleteAsync(int id);
    }
}