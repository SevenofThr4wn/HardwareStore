using HardwareStore.Core.Models;

namespace HardwareStore.Services.Interfaces
{
    public interface IProductService
    {

        Task<IEnumerable<Product>> GetAllAsync();
        Task<Product?> GetByIdAsync(int id);
        Task CreateAsync(Product product);
        Task UpdateAsync(Product product);
        Task DeleteAsync(int id);
    }
}
