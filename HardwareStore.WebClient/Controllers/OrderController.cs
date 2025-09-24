using HardwareStore.Core.Enums;
using HardwareStore.Core.Models;
using HardwareStore.Data.Context;
using HardwareStore.Data.Models.Interfaces;
using HardwareStore.WebClient.Services;
using HardwareStore.WebClient.ViewModels.Orders.Create;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HardwareStore.WebClient.Controllers
{
    public class OrderController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IOrderService _orderService;
        private readonly IOrderRepository _orderRepository;

        public OrderController(IOrderRepository orderRepository,
            IOrderService orderService,
            AppDbContext context)
        {
            _context = context;
            _orderService = orderService;
            _orderRepository = orderRepository;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrder(OrderCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var productIds = model.Items.Select(i => i.ProductId).ToList();
            var products = await _context.Products
                .Where(p => productIds.Contains(p.ProductId))
                .ToListAsync();

            if (products.Count != productIds.Count)
            {
                return BadRequest("One or more products do not exist");
            }

            foreach (var item in model.Items)
            {
                var product = products.First(p => p.ProductId == item.ProductId);
                if (product.StockQuantity < item.Quantity)
                {
                    return BadRequest($"Not enough stock for product {product.Name}.");
                }
            }

            var order = new Order
            {
                UserId = model.UserId,
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                TotalAmount = model.TotalAmount,
                OrderItems = model.Items.Select(i => new OrderItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    UnitPrice = products.First(p => p.ProductId == i.ProductId).Price,
                }).ToList()
            };
            await _orderRepository.AddAsync(order);

            // Deduct Stock
            foreach (var item in model.Items)
            {
                var product = products.First(p => p.ProductId == item.ProductId);
                product.StockQuantity -= item.Quantity;
            }

            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Order Sucessfully Created!", orderId = order.Id });
        }

        // GET: CancelOrder
        [HttpGet]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var order = await _orderService.GetOrderDetailsAsync(id, User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (order == null)
                return NotFound();

            return View(order);
        }

        // POST: CancelOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("CancelOrder")]
        public async Task<IActionResult> CancelOrderConfirmed(int id)
        {
            try
            {
                await _orderService.CancelOrderAsync(id);
                return RedirectToAction(nameof(MyOrders));
            }
            catch
            {
                var order = await _orderService.GetOrderDetailsAsync(id, User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                return View(order);
            }
        }

        [Authorize]
        public async Task<IActionResult> MyOrders(OrderStatus? statusFilter = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var orders = await _orderService.GetUserOrdersAsync(userId!, statusFilter);

            ViewBag.StatusFilter = statusFilter;
            ViewBag.AvailableStatuses = Enum.GetValues<OrderStatus>();

            return View(orders);
        }

        [Authorize]
        public async Task<IActionResult> OrderDetails(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var orderDetails = await _orderService.GetOrderDetailsAsync(id, userId!);

            if (orderDetails == null)
                return NotFound();

            return View(orderDetails);
        }

        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> ManageOrders(string statusFilter, DateTime? dateFilter)
        {
            var orders = await _orderService.GetOrdersAsync(statusFilter, dateFilter);

            ViewBag.StatusFilter = statusFilter;
            ViewBag.DateFilter = dateFilter;
            ViewBag.AvailableStatuses = new[] { "All", "Pending", "Processing", "Shipped", "Delivered", "Cancelled" };

            return View(orders);
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, string status)
        {
            if (!Enum.TryParse<OrderStatus>(status, out var parsedStatus))
                return BadRequest("Invalid order status");

            await _orderService.UpdateOrderStatusAsync(orderId, parsedStatus);

            return Ok(new { success = true, message = $"Order status updated to {parsedStatus}" });
        }
    }
}