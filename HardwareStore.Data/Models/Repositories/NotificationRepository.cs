using HardwareStore.Core.Models;
using HardwareStore.Data.Context;
using HardwareStore.Data.Models.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HardwareStore.Data.Models.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly AppDbContext _context;

        public NotificationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Notification>> GetAllAsync()
        {
            var notifications = await _context.Notifications.ToListAsync();
            return notifications;
        }

        public async Task<IEnumerable<Notification>> GetByUserIdAsync(string id)
        {
            var notification = await _context.Notifications
                            .Where(n => n.UserId == id)
                            .OrderByDescending(n => n.CreatedAt)
                            .ToListAsync();
            return notification;
        }

        public async Task AddAsync(Notification notification)
        {
            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Notification notification)
        {
            _context.Notifications.Update(notification);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Notification notification)
        {
            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();
        }
    }
}