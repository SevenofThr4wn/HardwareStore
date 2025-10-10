using HardwareStore.Core.Enums;
using HardwareStore.Core.Models;
using HardwareStore.Data.Context;
using HardwareStore.Data.Models.Interfaces;
using HardwareStore.Services.Interfaces;
using HardwareStore.WebClient.Services;
using HardwareStore.WebClient.ViewModels.Orders.Create;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HardwareStore.WebClient.Controllers
{
    /// <summary>
    /// Controller responsible for managing orders including creation, cancellation, viewing,
    /// and updating of order statuses. Integrates with notification and order services. 
    /// </summary>
    public class OrdersController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IOrderService _orderService;
        private readonly IOrderRepository _orderRepo;
        private readonly INotificationService _notificationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrdersController"/> class.
        /// </summary>
        /// <param name="orderRepo">The repository used for data access operations related to orders.</param>
        /// <param name="orderService">The service handling business logic related to orders.</param>
        /// <param name="notificationService">The service responsible for sending notifications.</param>
        /// <param name="context">The database context used for interacting with the SQL database.</param>
        public OrdersController(
            IOrderRepository orderRepo,
            IOrderService orderService,
            INotificationService notificationService,
            AppDbContext context)
        {
            _orderRepo = orderRepo;
            _orderService = orderService;
            _notificationService = notificationService;
            _context = context;
        }

        /// <summary>
        /// Creates a new order, validates stock, deducts quantities, and sends notifications.
        /// </summary>
        /// <param name="model">The view model containing order and item details.</param>
        /// <returns>A JSON response indicating success or failure.</returns>
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

            var user = await _context.Users.FindAsync(model.UserId);
            var userName = user?.UserName ?? "Unknown User";

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
            await _orderRepo.AddAsync(order);

            // Deduct Stock
            foreach (var item in model.Items)
            {
                var product = products.First(p => p.ProductId == item.ProductId);
                product.StockQuantity -= item.Quantity;

                if (product.StockQuantity <= 10) 
                {
                    await _notificationService.NotifyLowStock(
                        product.ProductId,
                        product.Name,
                        product.StockQuantity
                    );
                }
            }

            await _context.SaveChangesAsync();

            await _notificationService.NotifyNewOrder(
                order.Id,
                order.OrderNo,
                userName,
                order.TotalAmount
            );

            return Ok(new { success = true, message = "Order Sucessfully Created!", orderId = order.Id });
        }


        /// <summary>
        /// Displays the confirmation view before an order is canceled.
        /// </summary>
        /// <param name="id">The ID of the order to cancel.</param>
        /// <returns>The cancellation confirmation view.</returns>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var order = await _orderService.GetOrderDetailsAsync(id, User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (order == null)
                return NotFound();

            return View(order);
        }

        /// <summary>
        /// Confirms and processes the cancellation of an existing order.
        /// </summary>
        /// <param name="id">The ID of the order to cancel.</param>
        /// <returns>A redirect to the My Orders page upon success.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("CancelOrder")]
        public async Task<IActionResult> CancelOrderConfirmed(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var currentUser = User.Identity?.Name ?? "System";

                var order = await _orderService.GetOrderDetailsAsync(id, userId!);
                if (order == null)
                    return NotFound();

                var oldStatus = order.Status.ToString();

                await _orderService.CancelOrderAsync(id);

                await _notificationService.NotifyOrderUpdateStatus(
                    id,
                    order.OrderNumber,
                    oldStatus,
                    "Cancelled",
                    currentUser
                );

                return RedirectToAction(nameof(MyOrders));
            }
            catch
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var order = await _orderService.GetOrderDetailsAsync(id, userId!);
                return View(order);
            }
        }

        /// <summary>
        /// Displays a list of orders for the currently logged-in user, optionally filtered by status.
        /// </summary>
        /// <param name="statusFilter">Optional order status to filter by.</param>
        /// <returns>The My Orders view displaying user's orders.</returns>
        [Authorize]
        public async Task<IActionResult> MyOrders(OrderStatus? statusFilter = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var orders = await _orderService.GetUserOrdersAsync(userId!, statusFilter);

            ViewBag.StatusFilter = statusFilter;
            ViewBag.AvailableStatuses = Enum.GetValues<OrderStatus>();

            return View(orders);
        }

        /// <summary>
        /// Displays detailed information for a specific order.
        /// </summary>
        /// <param name="id">The ID of the order.</param>
        /// <returns>The order details view if found; otherwise a NotFound result.</returns>
        [Authorize]
        public async Task<IActionResult> OrderDetails(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var orderDetails = await _orderService.GetOrderDetailsAsync(id, userId!);

            if (orderDetails == null)
                return NotFound();

            ViewBag.OrderId = id;

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

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, string status)
        {
            if (!Enum.TryParse<OrderStatus>(status, out var parsedStatus))
                return BadRequest("Invalid order status");

            // Get current order status before update
            var currentOrder = await _orderRepo.GetByIdAsync(orderId);
            if (currentOrder == null)
                return NotFound();

            var oldStatus = currentOrder.Status.ToString();
            var currentUser = User.Identity?.Name ?? "System";

            await _orderService.UpdateOrderStatusAsync(orderId, parsedStatus);

            await _notificationService.NotifyOrderUpdateStatus(
                orderId,
                currentOrder.OrderNo,
                oldStatus,
                parsedStatus.ToString(),
                currentUser
            );

            return Ok(new { success = true, message = $"Order status updated to {parsedStatus}" });
        }
    }
}