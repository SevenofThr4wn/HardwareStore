using HardwareStore.Core.Models;
using HardwareStore.Data.Repositories.Interfaces;
using HardwareStore.Services.Interfaces;

namespace HardwareStore.Services.Implementations
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly INotificationService _notificationService;

        public ProductService(
            IProductRepository productRepository,
            INotificationService notificationService)
        {
            _productRepository = productRepository;
            _notificationService = notificationService;
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _productRepository.GetAllAsync();
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _productRepository.GetByIdAsync(id);
        }

        public async Task CreateAsync(Product product)
        {
            await _productRepository.AddAsync(product);
            await _notificationService.SendNotificationAsync($"New product created: {product.Name}");
        }

        public async Task UpdateAsync(Product product)
        {
            await _productRepository.UpdateAsync(product);
            await _notificationService.SendNotificationAsync($"Product updated: {product.Name}");
        }

        public async Task DeleteAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                throw new KeyNotFoundException($"Product with ID {id} not found.");

            await _productRepository.DeleteAsync(product);
            await _notificationService.SendNotificationAsync($"Product deleted: {product.Name}");
        }
    }
}
