using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HardwareStore.Core.Models
{
    public class OrderItem
    {
        [Key, Column("order_item_id")]
        public int Id { get; set; }

        [Column("order_id")]
        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;

        [Column("product_id")]
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        [Column("unit_price", TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Column("quantity")]
        public int Quantity { get; set; }
    }
}
