using HardwareStore.Core.Enums;
using HardwareStore.Core.Models;

namespace HardwareStore.Data.Models.Interfaces
{
    public interface IOrderRepository : IBaseRepository<Order>
    {
        Task UpdateOrderStatusAsync(int id, OrderStatus updatedStatus);
        Task CompleteOrderAsync(int id, OrderStatus updatedStatus);
    }
}