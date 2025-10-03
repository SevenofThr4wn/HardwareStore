using HardwareStore.Core.Models;
using HardwareStore.Data.Context;
using HardwareStore.Data.Models.Interfaces;

namespace HardwareStore.Data.Models.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
            Users = new UserRepository(_context);
            Orders = new OrderRepository(_context);
            Products = new BaseRepository<Product>(_context);
            Notifications = new BaseRepository<Notification>(_context);
        }

        public IUserRepository Users { get; private set; }
        public IOrderRepository Orders { get; private set; }
        public IBaseRepository<Product> Products { get; private set; }
        public IBaseRepository<Notification> Notifications { get; private set; }

        public async Task<int> CompleteAsync() => await _context.SaveChangesAsync();
        public void Dispose() => _context.Dispose();
    }
}
