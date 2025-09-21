using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HardwareStore.Core.Models
{
    public class OrderItem
    {
        [Key, Column("order_item_id")]
        public int Id { get; set; }

        [Required, Column("order_id")]
        public int OrderId { get; set; }

        [Required, Column("product_id")]
        public int ProductId { get; set; }

        [Required, Column("unit_price", TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Required, Column("quantity")]
        public int Quantity { get; set; }

        // Navigation Properties
        public Product? Product { get; set; }
        public Order? Order { get; set; }
    }
}
