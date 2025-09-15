using HardwareStore.Core.Models;
using HardwareStore.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HardwareStore.WebClient.Controllers
{

    [Authorize]
    public class ProductController : Controller
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }


        // GET: ProductController
        public async Task<ActionResult> Index()
        {
            var products = await _productService.GetAllAsync();
            return View(products);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: ProductController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> Create(Product product)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(product);

                await _productService.CreateAsync(product);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View(product);
            }
        }

        // GET: ProductController/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null) return NotFound();
            return View(product);
        }

        // POST: ProductController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product)
        {

            if (!ModelState.IsValid) return View(product);

            await _productService.UpdateAsync(product);
            return RedirectToAction(nameof(Index));
        }

        // GET: ProductController/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            await _productService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
