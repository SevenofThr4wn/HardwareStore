using System.ComponentModel.DataAnnotations;

namespace HardwareStore.WebClient.ViewModels.Orders.Create
{
    public class OrderCreateViewModel
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal TotalAmount { get; set; }

        [Required]
        public List<OrderItemCreateModel> Items { get; set; } = new();
    }
}
