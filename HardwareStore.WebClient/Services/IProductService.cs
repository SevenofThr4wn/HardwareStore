using HardwareStore.Core.Models;
using HardwareStore.WebClient.Models;
using HardwareStore.WebClient.ViewModels.Product.Create;
using HardwareStore.WebClient.ViewModels.Product.Edit;

namespace HardwareStore.WebClient.Services
{
    public interface IProductService
    {
        IQueryable<Product> GetProductsQuery();
        Task<int> GetTotalProducts();
        Task<int> GetLowStockCount(int threshold = 10);
        Task<List<ProductStockInfo>> GetLowStockItems(int take = 10);
        Task<List<string>> GetAvailableCategoriesAsync();
        Task<Product> CreateProductAsync(ProductEditVM model);
        Task<Product> CreateProductAsync(ProductCreateVM model);
        Task<Product?> UpdateProductAsync(ProductEditVM model);
        Task DeleteProductAsync(int productId);
    }
}