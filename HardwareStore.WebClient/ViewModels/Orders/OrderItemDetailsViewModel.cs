namespace HardwareStore.WebClient.ViewModels.Orders
{
    public class OrderItemDetailsViewModel
    {
        public string ProductName { get; set; } = string.Empty;
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public string? Description { get; set; }

        public decimal TotalPrice => Price * Quantity;
        public string FormattedPrice => Price.ToString("C");
        public string FormattedTotalPrice => TotalPrice.ToString("C");
    }
}
