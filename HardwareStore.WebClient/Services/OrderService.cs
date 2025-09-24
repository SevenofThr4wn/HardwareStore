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
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _context;
        private readonly IOrderRepository _orderRepository;

        public OrderService(AppDbContext context, IOrderRepository orderRepository)
        {
            _context = context;
            _orderRepository = orderRepository;
        }

        public IQueryable<Order> GetOrdersQuery()
        {
            return _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .AsQueryable();
        }

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

        public async Task UpdateOrderStatusAsync(int orderId, OrderStatus status)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null) throw new KeyNotFoundException("Order not found!");

            order.Status = status;
            order.OrderDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task CancelOrderAsync(int orderId)
        {
            await _orderRepository.DeleteAsync(orderId);
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetTodaysOrders()
        {
            return await _context.Orders.CountAsync(o => o.OrderDate.Date == DateTime.Today);
        }

        public async Task<decimal> GetTotalRevenue()
        {
            return await _context.Orders
                    .Where(o => o.OrderDate.Date == DateTime.Today)
                    .SumAsync(o => (int)o.TotalAmount);
        }

        public async Task<int> GetPendingOrdersCount()
        {
            return await _context.Orders
                .CountAsync(o => o.Status == OrderStatus.Pending);
        }

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