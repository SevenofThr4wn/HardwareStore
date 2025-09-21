using HardwareStore.Core.Models;
using HardwareStore.Data.Context;
using HardwareStore.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HardwareStore.Data.Repositories
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
            return await _context.Orders.ToListAsync();
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

        public async Task<int> DeleteAsync(int id)
        {
            var order = await _context.Orders
                .Where(b => b.Id == id)
                .ExecuteDeleteAsync();
            return order;
        }
    }
}