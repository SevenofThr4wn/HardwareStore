using HardwareStore.Core.Models;
using HardwareStore.Data.Context;
using HardwareStore.Data.Models.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace HardwareStore.Data.Models.Repositories
{
    /// <summary>
    /// Repository for managing <see cref="ApplicationUser"/> entities.
    /// Provides basic CRUD operations through <see cref="BaseRepository{T}"/> and
    /// additional methods for retrieving users by email or username, with caching support.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="UserRepository"/> class.
    /// </remarks>
    /// <param name="context">The <see cref="AppDbContext"/> instance to use for database operations.</param>
    /// <param name="memoryCache">The <see cref="IMemoryCache"/> instance to use for caching users.</param>
    public class UserRepository(AppDbContext context, IMemoryCache memoryCache) : BaseRepository<ApplicationUser>(context), IUserRepository
    {
        private readonly AppDbContext _context = context;
        private readonly IMemoryCache _cache = memoryCache;

        /// <summary>
        /// Retrieves a user by their email address asynchronously.
        /// If the user is already cached, the cached instance is returned.
        /// Otherwise, the user is queried from the database and cached for subsequent calls.
        /// </summary>
        /// <param name="email">The email address of the user to retrieve.</param>
        /// <returns>
        /// The <see cref="ApplicationUser"/> with the specified email if found; otherwise, <c>null</c>.
        /// </returns>
        public async Task<ApplicationUser?> GetByEmailAsync(string email)
        {
            if (!_cache.TryGetValue(email, out ApplicationUser? user))
            {
                user = await _context.AppUsers.SingleOrDefaultAsync(u => u.Email == email);
                if (user != null)
                {
                    _cache.Set(email, user, TimeSpan.FromMinutes(5));
                }
            }
            return user;
        }

        /// <summary>
        /// Retrieves a user by their username asynchronously.
        /// If the user is already cached, the cached instance is returned.
        /// Otherwise, the user is queried from the database and cached for subsequent calls.
        /// </summary>
        /// <param name="username">The username of the user to retrieve.</param>
        /// <returns>
        /// The <see cref="ApplicationUser"/> with the specified username if found; otherwise, <c>null</c>.
        /// </returns>
        public async Task<ApplicationUser?> GetByUsernameAsync(string username)
        {
            if (_cache.TryGetValue(username, out ApplicationUser? user))
            {
                return user;
            }

            user = await _context.AppUsers.FirstOrDefaultAsync(u => u.UserName == username);

            if (user != null)
            {
                _cache.Set(username, user, TimeSpan.FromMinutes(5));
            }

            return user;
        }
    }
}