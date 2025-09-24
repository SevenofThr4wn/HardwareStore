using HardwareStore.Core.Models;
using HardwareStore.Data.Models.Interfaces;
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

        /// <summary>
        /// Retrieves the user's notifications by a provided record identifier.
        /// </summary>
        /// <param name="userId">The identifier of the user.</param>
        /// <returns>The user object that matches the record id.</returns>
        public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(string id)
        {
            return await _notificationRepository.GetByUserIdAsync(id);
        }

        /// <summary>
        /// Sends a <see cref="Notification"/> to a <see cref="ApplicationUser"/> that is authenticated.
        /// </summary>
        /// <param name="id">The Id of the user that the notif will be sent to.</param>
        /// <param name="message">The message that will be passed as the body of the notification.</param>
        /// <returns></returns>
        public async Task SendNotificationAsync(string id, string? title, string message)
        {
            var notification = new Notification
            {
                UserId = id,
                Title = title,
                Message = message
            };

            await _notificationRepository.AddAsync(notification);

            await _publisher.PublishAsync(message, title, id);
        }

        public async Task SendNotificationAsync(string? title, string message)
        {
            var notification = new Notification
            {
                UserId = null,
                Message = message
            };

            await _notificationRepository.AddAsync(notification);

            await _publisher.PublishAsync(message, title, null!);
        }
    }
}