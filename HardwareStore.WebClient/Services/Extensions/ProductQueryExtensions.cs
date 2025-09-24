using HardwareStore.Core.Models;
using HardwareStore.WebClient.ViewModels.Product.Edit;
using HardwareStore.WebClient.ViewModels.Product.Manage;

namespace HardwareStore.WebClient.Services.Extensions
{
    public static class ProductQueryExtensions
    {
        public static IQueryable<Product> ApplySearch(
            this IQueryable<Product> query, string? searchString)
        {
            if (string.IsNullOrEmpty(searchString))
                return query;

            return query.Where(p =>
                p.Name.Contains(searchString) ||
                p.Description.Contains(searchString)
            );
        }

        public static IQueryable<Product> ApplyCategoryFilter(
            this IQueryable<Product> query, string? categoryFilter)
        {
            if (!string.IsNullOrEmpty(categoryFilter) && categoryFilter != "All")
            {
                query = query.Where(p => p.Category == categoryFilter);
            }
            return query;
        }

        public static IQueryable<ProductManageVM> SelectProducts(
            this IQueryable<Product> query)
        {
            return query.Select(p => new ProductManageVM
            {
                Id = p.ProductId,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                Category = p.Category,
                IsActive = p.IsActive,
                CreatedDate = p.CreatedDate
            });
        }

        public static IQueryable<ProductEditVM> SelectProductEdit(this IQueryable<Product> query)
        {
            return query.Select(p => new ProductEditVM
            {
                Id = p.ProductId,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                Category = p.Category,
                IsActive = p.IsActive
            });
        }
    }
}