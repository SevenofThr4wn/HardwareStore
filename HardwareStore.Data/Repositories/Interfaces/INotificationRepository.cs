using HardwareStore.Core.Models;

namespace HardwareStore.Data.Repositories.Interfaces
{
    public interface INotificationRepository
    {
        Task<IEnumerable<Notification>> GetByUserIdAsync(string userId);
        Task AddAsync(Notification notification);
        Task UpdateAsync(Notification notification);
        Task DeleteAsync(Notification notification);
    }
}
