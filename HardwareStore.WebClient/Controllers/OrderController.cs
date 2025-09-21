using HardwareStore.Data.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HardwareStore.WebClient.Controllers
{
    public class OrderController : Controller
    {
        private readonly IOrderRepository _orderRepository;

        public OrderController(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        [HttpGet]
        public async Task<IActionResult> ManageOrder(int id)
        {
            var selectedOrder = await _orderRepository.GetByIdAsync(id);
            return View(selectedOrder);
        }

        // GET: CancelOrder
        [HttpGet]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var selectedOrder = await _orderRepository.GetByIdAsync(id);
            return View(selectedOrder);
        }

        // POST: CancelOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("CancelOrder")]
        public async Task<IActionResult> CancelOrderConfirmed(int id)
        {
            try
            {
                await _orderRepository.DeleteAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                var selectedOrder = await _orderRepository.GetByIdAsync(id);
                return View(selectedOrder);
            }
        }
    }
}
