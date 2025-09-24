using HardwareStore.Core.Models;

namespace HardwareStore.Services.Interfaces
{
    public interface INotificationService
    {
        Task SendNotificationAsync(string? title, string message);
        Task SendNotificationAsync(string id, string? title, string message);
        Task<IEnumerable<Notification>> GetUserNotificationsAsync(string id);
    }
}