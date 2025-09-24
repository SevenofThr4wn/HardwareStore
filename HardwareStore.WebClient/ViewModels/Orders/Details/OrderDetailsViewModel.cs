using HardwareStore.Core.Enums;

namespace HardwareStore.WebClient.ViewModels.Orders.Details
{
    public class OrderDetailsViewModel
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; }
        public string? ShippingAddress { get; set; }
        public string? ShippingCity { get; set; }
        public string? ShippingPostalCode { get; set; }
        public string? ShippingCountry { get; set; }
        public List<OrderItemDetailsViewModel> Items { get; set; } = new List<OrderItemDetailsViewModel>();
        public OrderUserViewModel User { get; set; } = new OrderUserViewModel();

        public string StatusBadgeClass => Status switch
        {
            OrderStatus.Pending => "bg-warning",
            OrderStatus.Processing => "bg-info",
            OrderStatus.Shipped => "bg-primary",
            OrderStatus.Delivered => "bg-success",
            OrderStatus.Cancelled => "bg-danger",
            _ => "bg-secondary"
        };
    }
}
