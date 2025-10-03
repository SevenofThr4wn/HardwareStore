using HardwareStore.Core.Models;

namespace HardwareStore.Data.Models.Interfaces
{
    public interface INotificationRepository : IBaseRepository<Notification>
    {
        Task<IEnumerable<Notification>> GetByUserIdAsync(string id);

    }
}