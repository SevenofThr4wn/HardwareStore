using HardwareStore.Core.Enums;

namespace HardwareStore.WebClient.ViewModels
{
    public class OrderViewModel
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public double Total { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        public List<OrderItemViewModel> Items { get; set; } = new();
    }
}