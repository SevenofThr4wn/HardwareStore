using HardwareStore.Services.Hubs;
using HardwareStore.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace HardwareStore.Services.Implementations
{
    /// <summary>
    /// Service for sending real-time notifications via SignalR to clients and groups.
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationService"/> class.
        /// </summary>
        /// <param name="hubContext">The SignalR hub context to use for notifications.</param>
        public NotificationService(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        /// <summary>
        /// Sends a notification to clients about an order status update.
        /// </summary>
        /// <param name="orderId">The ID of the order.</param>
        /// <param name="orderNumber">The order number.</param>
        /// <param name="oldStatus">The previous status of the order.</param>
        /// <param name="newStatus">The new status of the order.</param>
        /// <param name="updatedBy">The user who updated the order.</param>
        public async Task NotifyOrderUpdateStatus(int orderId, string orderNumber, string oldStatus, string newStatus, string updatedBy)
        {
            var message = $"Order #{orderNumber} status changed from {oldStatus} to {newStatus} by {updatedBy}";

            await _hubContext.Clients.Group($"order-{orderId}")
                .SendAsync("RecieveOrderUpdate", new
                {
                    OrderId = orderId,
                    OrderNumber = orderNumber,
                    OldStatus = oldStatus,
                    NewStatus = newStatus,
                    UpdatedBy = updatedBy,
                    Message = message,
                    Timestamp = DateTime.UtcNow

                });

            await _hubContext.Clients.Groups("admin-notifications", "manager-notifications")
                .SendAsync("RecieveNotification", new
                {
                    Title = "Order Status Updated",
                    Message = message,
                    Type = "info",
                    TimeStamp = DateTime.UtcNow
                });
        }

        /// <summary>
        /// Sends a notification about a new order to admin and manager groups.
        /// </summary>
        /// <param name="orderId">The ID of the order.</param>
        /// <param name="orderNumber">The order number.</param>
        /// <param name="customerName">The name of the customer.</param>
        /// <param name="totalAmount">The total amount of the order.</param>
        public async Task NotifyNewOrder(int orderId, string orderNumber, string customerName, decimal totalAmount)
        {
            var message = $"New Order #{orderNumber} from {customerName} for {totalAmount:C}";

            await _hubContext.Clients.Groups("admin-notifications", "manager-notifications")
                .SendAsync("RecieveNotification", new
                {
                    Title = "New Order",
                    Message = message,
                    Type = "Success",
                    OrderId = orderId,
                    TimeStamp = DateTime.UtcNow
                });
        }

        /// <summary>
        /// Sends a low stock alert to admin and manager groups.
        /// </summary>
        /// <param name="productId">The ID of the product.</param>
        /// <param name="productName">The name of the product.</param>
        /// <param name="currentStock">The curent stock level of the product.</param>
        public async Task NotifyLowStock(int productId, string productName, int currentStock)
        {
            var message = $"Product '{productName}' is low on stock. Current: {currentStock}";

            await _hubContext.Clients.Groups("admin-notifications", "manager-notifications")
                .SendAsync("ReceiveNotification", new
                {
                    Title = "Low Stock Alert",
                    Message = message,
                    Type = "warning",
                    ProductId = productId,
                    Timestamp = DateTime.UtcNow
                });
        }

        /// <summary>
        /// Sends a custom notification to the admin group.
        /// </summary>
        /// <param name="title">The title of the notification.</param>
        /// <param name="message">The notification message.</param>
        /// <param name="type">The type of notification (e.g., info, warning, success). Defaults to "info"</param>
        public async Task NotifyAdmin(string title, string message, string type = "info")
        {
            await _hubContext.Clients.Group("admin-notifications")
                .SendAsync("ReceiveNotification", new
                {
                    Title = title,
                    Message = message,
                    Type = type,
                    Timestamp = DateTime.UtcNow
                });
        }
    }
}   