using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HardwareStore.Core.Models
{
    [Index(nameof(OrderId), nameof(ProductId), IsUnique = true)]
    public class OrderItem
    {
        [Key]
        [Column("OrderItemId")]
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Unit price must be positive.")]
        public decimal UnitPrice { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }

        // Navigation Properties
        public Product? Product { get; set; }
        public Order? Order { get; set; }
    }
}