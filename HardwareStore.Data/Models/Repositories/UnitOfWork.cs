using HardwareStore.Core.Models;
using HardwareStore.Data.Context;
using HardwareStore.Data.Models.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace HardwareStore.Data.Models.Repositories
{
    /// <summary>
    /// Implements the Unit of Work pattern to manage multiple repositories and commit changes as a single transaction.
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnitOfWork"/> class and sets up repositories.
        /// </summary>
        /// <param name="context">The database context to use.</param>
        /// <param name="memoryCache">The memory cache instance to pass to repositories that require caching.</param>
        public UnitOfWork(AppDbContext context, IMemoryCache memoryCache)
        {
            _context = context;
            _cache = memoryCache;
            Users = new UserRepository(_context, _cache);
            Orders = new OrderRepository(_context);
            Products = new BaseRepository<Product>(_context);
            Notifications = new BaseRepository<Notification>(_context);
        }

        /// <summary>
        /// Repository for managing users.
        /// </summary>
        public IUserRepository Users { get; private set; }

        /// <summary>
        /// Repository for managing orders.
        /// </summary>
        public IOrderRepository Orders { get; private set; }

        /// <summary>
        /// Repository for managing products. 
        /// </summary>
        public IBaseRepository<Product> Products { get; private set; }

        /// <summary>
        /// Repository for managing notifications. 
        /// </summary>
        public IBaseRepository<Notification> Notifications { get; private set; }

        /// <summary>
        /// Saves all changes made in the current unit of work to the database asynchronously.
        /// </summary>
        /// <returns>The number of state entries written to the database.</returns>
        public async Task<int> CompleteAsync() => await _context.SaveChangesAsync();

        /// <summary>
        /// Disposes the underlying database context.
        /// </summary>
        public void Dispose() => _context.Dispose();
    }
}
