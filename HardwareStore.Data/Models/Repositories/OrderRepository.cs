using HardwareStore.Core.Enums;
using HardwareStore.Core.Models;
using HardwareStore.Data.Context;
using HardwareStore.Data.Models.Interfaces;

namespace HardwareStore.Data.Models.Repositories
{
    public class OrderRepository : BaseRepository<Order>, IOrderRepository
    {
        public OrderRepository(AppDbContext context) : base(context) { }

        /// <summary>
        /// Marks an order as complete by updating its status to 'Shipped'.
        /// </summary>
        /// <param name="id">The ID of the order to complete.</param>
        /// <param name="updatedStatus">The order status to update to.</param>
        public async Task CompleteOrderAsync(int id, OrderStatus updatedStatus)
        {
            var order = await GetByIdAsync(id);

            if (order == null)
                return;

            order.Status = OrderStatus.Shipped;
            _context.Update(order);
        }

        /// <summary>
        /// Updates the status of an order asynchronously.
        /// </summary>
        /// <param name="id">The ID of the order to update.</param>
        /// <param name="updatedStatus">The order status to update to.</param>
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