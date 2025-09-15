using HardwareStore.Core.Models;
using HardwareStore.Data.Repositories.Interfaces;
using HardwareStore.Services.Interfaces;

namespace HardwareStore.Services.Implementations
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly INotificationPublisher _publisher;

        public NotificationService(
            INotificationRepository notificationRepository,
            INotificationPublisher publisher)
        {
            _notificationRepository = notificationRepository;
            _publisher = publisher;
        }

        public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId)
        {
            return await _notificationRepository.GetByUserIdAsync(userId);
        }

        public async Task SendNotificationAsync(string userId, string message)
        {
            var notification = new Notification
            {
                UserId = userId,
                Message = message
            };

            await _notificationRepository.AddAsync(notification);

            await _publisher.PublishAsync(message, userId);
        }

        public async Task SendNotificationAsync(string message)
        {
            var notification = new Notification
            {
                UserId = null,
                Message = message
            };

            await _notificationRepository.AddAsync(notification);

            await _publisher.PublishAsync(message, null);
        }
    }
}
