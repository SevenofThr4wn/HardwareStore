using HardwareStore.Core.Enums;

namespace HardwareStore.WebClient.ViewModels.Orders
{
    public class OrderViewModel
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; }
        public int ItemCount { get; set; }
        public List<OrderItemViewModel> Items { get; set; } = new List<OrderItemViewModel>();

        public string FormattedTotalAmount => TotalAmount.ToString("C");
        public string FormattedOrderDate => OrderDate.ToString("MMM dd, yyyy");

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
