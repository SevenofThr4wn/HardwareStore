namespace HardwareStore.WebClient.Models
{
    public class OrderInfo
    {
        public int OrderId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public double Amount { get; set; }
    }
}
