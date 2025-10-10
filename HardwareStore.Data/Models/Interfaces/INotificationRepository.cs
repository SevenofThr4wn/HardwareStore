using HardwareStore.Core.Models;

namespace HardwareStore.Data.Models.Interfaces
{
    public interface INotificationRepository : IBaseRepository<Notification>
    {
        Task<List<Notification>> GetByUserIdAsync(string id);
    }
}