using HardwareStore.Core.Models;
using HardwareStore.Data.Context;
using HardwareStore.Data.Models.Interfaces;
using HardwareStore.WebClient.Models;
using HardwareStore.WebClient.ViewModels.Product.Create;
using HardwareStore.WebClient.ViewModels.Product.Edit;
using Microsoft.EntityFrameworkCore;

namespace HardwareStore.WebClient.Services
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _context;
        private readonly IUnitOfWork _unitOfWork;

        public ProductService(AppDbContext context, IUnitOfWork unitOfWork)
        {
            _context = context;
            _unitOfWork = unitOfWork;
        }

        public IQueryable<Product> GetProductsQuery()
        {
            return _context.Products.AsQueryable();
        }

        public async Task<List<string>> GetAvailableCategoriesAsync()
        {
            return await _context.Products
                .Select(p => p.Category)
                .Distinct()
                .ToListAsync();
        }

        public async Task<Product> CreateProductAsync(ProductEditVM model)
        {
            var product = new Product
            {
                Name = model.Name,
                Description = model.Description,
                Price = model.Price,
                StockQuantity = model.StockQuantity,
                Category = model.Category,
                IsActive = model.IsActive,
                CreatedDate = DateTime.UtcNow
            };

            await _unitOfWork.Products.AddAsync(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<Product?> UpdateProductAsync(ProductEditVM model)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(model.Id);
            if (product == null) return null;

            product.Name = model.Name;
            product.Description = model.Description;
            product.Price = model.Price;
            product.StockQuantity = model.StockQuantity;
            product.Category = model.Category;
            product.IsActive = model.IsActive;

            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<Product> CreateProductAsync(ProductCreateVM model)
        {
            var product = new Product
            {
                Name = model.Name,
                Description = model.Description,
                Price = model.Price,
                StockQuantity = model.StockQuantity,
                Category = model.Category,
                ImageUrl = model.ImageUrl,
                IsActive = model.IsActive,
                CreatedDate = DateTime.UtcNow
            };

            await _unitOfWork.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            return product;
        }

        public async Task DeleteProductAsync(int productId)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(productId);
            if (product == null)
                throw new KeyNotFoundException("Product not found");

            await _unitOfWork.Products.DeleteAsync(product);
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetTotalProducts()
        {
            return await _context.Products.CountAsync();
        }


        public async Task<int> GetLowStockCount(int threshold = 10)
        {
            return await _context.Products.CountAsync(p => p.StockQuantity < threshold);
        }

        public async Task<List<ProductStockInfo>> GetLowStockItems(int take = 10)
        {
            return await _context.Products
                .Where(p => p.StockQuantity < 10)
                .OrderBy(p => p.StockQuantity)
                .Take(take)
                .Select(p => new ProductStockInfo
                {
                    Name = p.Name,
                    Stock = p.StockQuantity
                })
                .ToListAsync();
        }
    }
}