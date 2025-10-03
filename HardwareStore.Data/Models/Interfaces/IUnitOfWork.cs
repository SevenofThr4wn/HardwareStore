using HardwareStore.Core.Models;

namespace HardwareStore.Data.Models.Interfaces
{
    public interface IUnitOfWork
    {
        IBaseRepository<Product> Products { get; }
        IBaseRepository<Notification> Notifications { get; }
        IUserRepository Users { get; }
        IOrderRepository Orders { get; }
        Task<int> CompleteAsync();
    }
}