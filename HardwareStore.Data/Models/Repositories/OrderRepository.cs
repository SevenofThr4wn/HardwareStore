using HardwareStore.Core.Enums;
using HardwareStore.Core.Models;
using HardwareStore.Data.Context;
using HardwareStore.Data.Models.Interfaces;

namespace HardwareStore.Data.Models.Repositories
{
    public class OrderRepository : BaseRepository<Order>, IOrderRepository
    {
        private readonly AppDbContext _context;

        public OrderRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task CompleteOrderAsync(int id, OrderStatus updatedStatus)
        {
            var order = await GetByIdAsync(id);

            if (order == null)
                return;

            order.Status = OrderStatus.Shipped;
            _context.Update(order);
        }

        public async Task UpdateOrderStatusAsync(int id, OrderStatus updatedStatus)
        {
            var order = await GetByIdAsync(id);

            if (order == null)
                return;

            order.Status = updatedStatus;
            _context.Update(order);
        }
    }
}