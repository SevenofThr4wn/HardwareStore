namespace HardwareStore.WebClient.ViewModels
{
    public class OrderItemViewModel
    {
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public double Price { get; set; }
        public double Subtotal => Quantity * Price;
    }
}