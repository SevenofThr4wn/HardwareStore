using HardwareStore.Core.Models;
using HardwareStore.Data.Context;
using HardwareStore.WebClient.Services;
using HardwareStore.WebClient.Services.Extensions;
using HardwareStore.WebClient.ViewModels.Product.Create;
using HardwareStore.WebClient.ViewModels.Product.Edit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HardwareStore.WebClient.Controllers
{
    public class ProductsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IProductService _productService;

        public ProductsController(AppDbContext context,
            IProductService productService)
        {
            _context = context;
            _productService = productService;
        }

        // GET: Create Product page
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> CreateProduct()
        {
            var categories = await _context.Products
                .Where(p => !string.IsNullOrEmpty(p.Category))
                .Select(p => p.Category)
                .Distinct()
                .ToListAsync();

            ViewBag.Categories = new SelectList(categories);

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> CreateProduct(ProductCreateVM model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(await _productService.GetAvailableCategoriesAsync());
                return View(model);
            }

            try
            {
                var product = await _productService.CreateProductAsync(model);
                TempData["SuccessMessage"] = $"Product '{product.Name}' created successfully!";
                return RedirectToAction("ManageProducts", "Products");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error creating product: {ex.Message}");
                ViewBag.Categories = new SelectList(await _productService.GetAvailableCategoriesAsync());
                return View(model);
            }
        }


        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> ManageProducts(string searchString, string categoryFilter)
        {
            var productsQuery = _productService.GetProductsQuery()
                .ApplySearch(searchString)
                .ApplyCategoryFilter(categoryFilter);

            var products = await productsQuery
                .OrderBy(p => p.Name)
                .SelectProducts()
                .ToListAsync();

            ViewBag.SearchString = searchString;
            ViewBag.CategoryFilter = categoryFilter;
            ViewBag.AvailableCategories = await _productService.GetAvailableCategoriesAsync();

            return View(products);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetProduct(int id)
        {
            var product = await _productService.GetProductsQuery()
                .Where(p => p.ProductId == id)
                .SelectProductEdit()
                .FirstOrDefaultAsync();

            if (product == null)
                return NotFound();

            return Ok(product);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> SaveProduct(ProductEditVM model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            Product? product;
            if (model.Id == 0)
            {
                product = await _productService.CreateProductAsync(model);
            }
            else
            {
                product = await _productService.UpdateProductAsync(model);
                if (product == null)
                    return NotFound();
            }

            return Ok(new { success = true, message = "Product saved successfully" });
        }


        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                await _productService.DeleteProductAsync(id);
                return Ok(new { success = true, message = "Product deleted successfully" });
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = $"Error deleting product: {ex.Message}" });
            }
        }
    }
}