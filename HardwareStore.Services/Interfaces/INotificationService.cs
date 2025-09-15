using HardwareStore.Core.Models;

namespace HardwareStore.Services.Interfaces
{
    public interface INotificationService
    {
        Task SendNotificationAsync(string userId, string message);
        Task SendNotificationAsync(string message);
        Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId);
    }
}