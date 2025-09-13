using HardwareStore.Core.Enums;
using HardwareStore.Core.Models;
using HardwareStore.Data.Repositories.Interfaces;
using HardwareStore.Services.Interfaces;

namespace HardwareStore.Services.Implementations
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly INotificationService _notificationService;

        public OrderService(IOrderRepository orderRepository, INotificationService notificationService)
        {
            _orderRepository = orderRepository;
            _notificationService = notificationService;
        }

        public async Task CreateOrderAsync(Order order)
        {
            // Add order to database
            await _orderRepository.AddAsync(order);

            // Send notification to the user who created the order
            if (!string.IsNullOrEmpty(order.UserId))
            {
                await _notificationService.SendNotificationAsync(
                    order.UserId,
                    $"New order created with ID: {order.Id}"
                );
            }
        }

        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            return await _orderRepository.GetAllAsync();
        }

        public async Task<Order> GetOrderByIDAsync(int id)
        {
            return await _orderRepository.GetByIdAsync(id);
        }

        public async Task UpdateOrderStatusAsync(int orderId, string status)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null) return;

            order.Status = Enum.Parse<OrderStatus>(status);
            await _orderRepository.UpdateAsync(order);

            await _notificationService.SendNotificationAsync(order.UserId, $"Order #{order.Id} status updated to {status}.");
        }
    }
}