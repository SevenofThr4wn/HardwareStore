using HardwareStore.Core.Enums;
using HardwareStore.Core.Models;
using HardwareStore.Data.Context;
using HardwareStore.Data.Models.Interfaces;
using HardwareStore.WebClient.ViewModels.Orders;
using HardwareStore.WebClient.ViewModels.Orders.Details;
using HardwareStore.WebClient.ViewModels.Orders.Manage;
using Microsoft.EntityFrameworkCore;

namespace HardwareStore.WebClient.Services
{

    /// <summary>
    /// Provides order-related operations for the web client, including retrieval,
    /// updating status, cancellation, and dashboard metrics.
    /// </summary>
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _context;
        private readonly IOrderRepository _orderRepository;

        public OrderService(AppDbContext context, IOrderRepository orderRepository)
        {
            _context = context;
            _orderRepository = orderRepository;
        }

        /// <summary>
        /// Returns a queryable collection of orders including related users and products.
        /// </summary>
        public IQueryable<Order> GetOrdersQuery()
        {
            return _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .AsQueryable();
        }

        /// <summary>
        /// Retrieves a list of orders for management purposes with optional status and date filters.
        /// Maps orders to <see cref="OrderManageVM"/> for dashboard or admin views.
        /// </summary>
        /// <param name="statusFilter">Optional status filter as string. Use "All" to include all orders.</param>
        /// <param name="dateFilter">Optional date filter to select orders by order date.</param>
        /// <returns>A list of <see cref="OrderManageVM"/> representing the filtered orders.</returns>
        public async Task<List<OrderManageVM>> GetOrdersAsync(string? statusFilter = null, DateTime? dateFilter = null)
        {
            var query = GetOrdersQuery();

            if (string.IsNullOrEmpty(statusFilter) && statusFilter != "All")
                query = query.Where(o => o.Status.ToString() == statusFilter);

            if (dateFilter.HasValue)
                query = query.Where(o => o.OrderDate.Date == dateFilter.Value.Date);

            return await query
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new OrderManageVM
                {
                    Id = o.Id,
                    OrderNumber = o.FormattedOrderNumber,
                    CustomerName = o.User!.UserName!,
                    CustomerEmail = o.User.Email!,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status.ToString(),
                    FullAddress = o.FullShippingAddress,
                    Items = o.OrderItems.Select(oi => new OrderItemViewModel
                    {
                        ProductName = oi.Product!.Name,
                        Quantity = oi.Quantity,
                        Price = (double)oi.UnitPrice

                    }).ToList()
                }).ToListAsync();
        }

        /// <summary>
        /// Retrieves all orders for a specific user, optionally filtered by order status.
        /// </summary>
        /// <param name="userId">The ID of the user whose orders should be retrieved.</param>
        /// <param name="statusFilter">Optional <see cref="OrderStatus"/> filter.</param>
        /// <returns>A list of <see cref="OrderViewModel"/> representing the user's orders.</returns>
        public async Task<List<OrderViewModel>> GetUserOrdersAsync(string userId, OrderStatus? statusFilter = null)
        {
            var query = _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Where(o => o.UserId == userId);

            if (statusFilter.HasValue)
                query = query.Where(o => o.Status == statusFilter.Value);

            return await query
                .Select(o => new OrderViewModel
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNo,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status,
                    ItemCount = o.OrderItems.Sum(oi => oi.Quantity),
                    Items = o.OrderItems.Select(oi => new OrderItemViewModel
                    {
                        ProductName = oi.Product!.Name,
                        Quantity = oi.Quantity,
                        Price = (double)oi.UnitPrice
                    }).ToList(),
                }).ToListAsync();
        }

        /// <summary>
        /// Retrieves detailed information for a specific order, including order items and user info.
        /// </summary>
        /// <param name="orderId">The ID of the order.</param>
        /// <param name="userId">The ID of the user who owns the order.</param>
        /// <returns>An <see cref="OrderDetailsViewModel"/> with full order details, or <c>null</c> if the order is not found.</returns>
        public async Task<OrderDetailsViewModel?> GetOrderDetailsAsync(int orderId, string userId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

            if (order == null)
                return null;

            return new OrderDetailsViewModel
            {
                Id = order.Id,
                OrderNumber = order.OrderNo,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                ShippingAddress = order.ShippingAddress,
                ShippingCity = order.ShippingCity,
                ShippingPostalCode = order.ShippingPostalCode,
                ShippingCountry = order.ShippingCountry,
                Items = order.OrderItems.Select(oi => new OrderItemDetailsViewModel
                {
                    ProductName = oi.Product!.Name,
                    ProductId = oi.Product.ProductId,
                    Quantity = oi.Quantity,
                    Price = oi.UnitPrice,
                    ImageUrl = oi.Product.ImageUrl,
                    Description = oi.Product.Description
                }).ToList(),

                User = new OrderUserViewModel
                {
                    UserName = order.User!.UserName!,
                    Email = order.User.Email!,
                    FirstName = order.User.FirstName!,
                    LastName = order.User.LastName!
                }
            };
        }

        /// <summary>
        /// Updates the status of a specific order.
        /// </summary>
        /// <param name="orderId">The ID of the order to update.</param>
        /// <param name="status">The new <see cref="OrderStatus"/> to apply.</param>
        /// <exception cref="KeyNotFoundException">Thrown if the order is not found.</exception>
        public async Task UpdateOrderStatusAsync(int orderId, OrderStatus status)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null) throw new KeyNotFoundException("Order not found!");

            order.Status = status;
            order.OrderDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Cancels a specific order and updates the modification date.
        /// </summary>
        /// <param name="orderId">The ID of the order to cancel.</param>
        public async Task CancelOrderAsync(int orderId)
        {
            await _orderRepository.UpdateAsync(orderId, order =>
            {
                order.Status = OrderStatus.Cancelled;
                order.UpdatedDate = DateTime.UtcNow;
            });

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Returns the total number of orders created today.
        /// </summary>
        public async Task<int> GetTodaysOrders()
        {
            return await _context.Orders.CountAsync(o => o.OrderDate.Date == DateTime.Today);
        }

        /// <summary>
        /// Returns the total revenue generated from orders today.
        /// </summary>
        public async Task<decimal> GetTotalRevenue()
        {
            return await _context.Orders
                    .Where(o => o.OrderDate.Date == DateTime.Today)
                    .SumAsync(o => (int)o.TotalAmount);
        }

        /// <summary>
        /// Returns the count of orders that are currently pending.
        /// </summary>
        public async Task<int> GetPendingOrdersCount()
        {
            return await _context.Orders
                .CountAsync(o => o.Status == OrderStatus.Pending);
        }

        /// <summary>
        /// Retrieves a list of recent activities from the activity log, ordered by timestamp descending.
        /// </summary>
        /// <param name="take">The maximum number of recent activities to retrieve.</param>
        /// <returns>A list of <see cref="ActivityLog"/> representing recent system activities.</returns>
        public async Task<List<ActivityLog>> GetRecentActivities(int take = 5)
        {
            return await _context.ActivityLogs
                .OrderByDescending(a => a.Timestamp)
                .Take(take)
                .Select(a => new ActivityLog
                {
                    Icon = a.Icon,
                    Text = a.Text,
                    Timestamp = a.Timestamp
                }).ToListAsync();
        }
    }
}