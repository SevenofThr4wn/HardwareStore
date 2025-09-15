using HardwareStore.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HardwareStore.WebClient.Controllers
{
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public async Task<IActionResult> TrackOrder(int orderId)
        {
            var order = await _orderService.GetOrderByIDAsync(orderId);
            return View(order);
        }

        public async Task<IActionResult> ManageCompletedOrder(int orderId)
        {
            var completedOrder = await _orderService.GetOrderByIDAsync(orderId);
            return View(completedOrder);
        }
    }
}
