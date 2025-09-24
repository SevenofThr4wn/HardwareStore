using System.ComponentModel.DataAnnotations;

namespace HardwareStore.WebClient.ViewModels.Orders.Create
{
    public class OrderItemCreateModel
    {
        [Required]
        public int ProductId { get; set; }

        public string? ProductName { get; set; }

        public double? Price { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
    }
}