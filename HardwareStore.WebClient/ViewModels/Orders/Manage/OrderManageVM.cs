namespace HardwareStore.WebClient.ViewModels.Orders.Manage
{
    public class OrderManageVM
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;

        public string FullAddress { get; set; } = string.Empty;
        public List<OrderItemViewModel> Items { get; set; } = new();
    }
}