namespace HardwareStore.Services.Interfaces
{
    public interface INotificationService
    {
        Task NotifyOrderUpdateStatus(int orderId, string orderNumber, string oldStatus, string newStatus, string updatedBy);
        Task NotifyNewOrder(int orderId, string orderNumber, string customerName, decimal totalAmount);
        Task NotifyLowStock(int productId, string productName, int currentStock);
        Task NotifyAdmin(string title, string message, string type = "info");
    }
}