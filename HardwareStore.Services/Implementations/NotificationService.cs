using HardwareStore.Services.Hubs;
using HardwareStore.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace HardwareStore.Services.Implementations
{
    public class NotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

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