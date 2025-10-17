using HardwareStore.Core.Models;

namespace HardwareStore.Data.Models.Interfaces
{
    public interface IUnitOfWork
    {
        IBaseRepository<Product> Products { get; }
        IUserRepository Users { get; }
        IOrderRepository Orders { get; }
        Task<int> CompleteAsync();
    }
}