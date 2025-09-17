using HardwareStore.Core.Models;
using HardwareStore.Data.Context;
using HardwareStore.Services.Interfaces;
using HardwareStore.WebClient.ViewModels.Product;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HardwareStore.WebClient.Controllers
{

    [Authorize]
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly AppDbContext _context;

        public ProductController(IProductService productService, AppDbContext context)
        {
            _context = context;
            _productService = productService;
        }


        // GET: Create Product page
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> CreateProduct()
        {
            // Get existing categories for dropdown
            var categories = await _context.Products
                .Where(p => !string.IsNullOrEmpty(p.Category))
                .Select(p => p.Category)
                .Distinct()
                .ToListAsync();

            ViewBag.Categories = new SelectList(categories);

            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProduct(ProductCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Reload categories if validation fails
                var categories = await _context.Products
                    .Where(p => !string.IsNullOrEmpty(p.Category))
                    .Select(p => p.Category)
                    .Distinct()
                    .ToListAsync();

                ViewBag.Categories = new SelectList(categories);
                return View(model);
            }

            try
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

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Product '{model.Name}' created successfully!";
                return RedirectToAction("ManageProducts", "Account");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty,
                    $"Error creating product: {ex.Message}");

                // Reload categories on error
                var categories = await _context.Products
                    .Where(p => !string.IsNullOrEmpty(p.Category))
                    .Select(p => p.Category)
                    .Distinct()
                    .ToListAsync();

                ViewBag.Categories = new SelectList(categories);
                return View(model);
            }
        }
    }
}
