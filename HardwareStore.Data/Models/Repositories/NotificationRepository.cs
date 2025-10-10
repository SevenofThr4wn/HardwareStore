using HardwareStore.Core.Models;
using HardwareStore.Data.Context;
using HardwareStore.Data.Models.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HardwareStore.Data.Models.Repositories
{
    /// <summary>
    /// Initalizes a new instance of the <see cref="NotificationRepository"/> class.
    /// </summary>
    /// <param name="context">The database context to use.</param>
    public class NotificationRepository(AppDbContext context) : BaseRepository<Notification>(context), INotificationRepository
    {
        private readonly AppDbContext _context = context;

        /// <summary>
        /// Retrieves notifications for a specific user, ordered by most recent first.
        /// </summary>
        /// <param name="id">The user ID to retrieve notifications for.</param>
        /// <returns>A list of notifications for the user.</returns>
        public Task<List<Notification>> GetByUserIdAsync(string id)
        {
            return _context.Notifications
                           .AsNoTracking()
                           .Where(n => n.UserId == id)
                           .OrderByDescending(n => n.CreatedAt)
                           .ToListAsync();
        }
    }
}