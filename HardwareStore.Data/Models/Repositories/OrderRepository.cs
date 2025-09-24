using HardwareStore.Core.Enums;
using HardwareStore.Core.Models;
using HardwareStore.Data.Context;
using HardwareStore.Data.Models.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HardwareStore.Data.Models.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _context;

        public OrderRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Order>> GetAllAsync()
        {
            var orders = await _context.Orders.ToListAsync();
            return orders;
        }

        public async Task<Order> GetByIdAsync(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            return order!;
        }

        public async Task AddAsync(Order order)
        {
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Order order)
        {
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Updates the order status of an order record.
        /// </summary>
        /// <param name="id">The id of the order record.</param>
        /// <param name="updatedStatus">The new status for the record.</param>
        /// <returns></returns>
        public async Task UpdateOrderStatusAsync(int id, OrderStatus updatedStatus)
        {
            var order = await GetByIdAsync(id);

            if (order == null)
                return;

            order.Status = updatedStatus;
            await UpdateAsync(order);
        }

        public async Task<int> DeleteAsync(int id)
        {
            var order = await _context.Orders
                .Where(b => b.Id == id)
                .ExecuteDeleteAsync();
            return order;
        }
    }
}