using HardwareStore.Core.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HardwareStore.Core.Models
{
    public class Order
    {
        [Key]
        [Column("order_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required, Column("user_id")]
        public string UserId { get; set; } = string.Empty;

        [Required, Column("order_number"), StringLength(50)]
        public string OrderNo { get; set; } = string.Empty;

        [Required, Column("total_amount", TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Required, Column("order_status")]
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        [Required, Column("order_date")]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Column("updated_date")]
        public DateTime? UpdatedDate { get; set; }

        // Shipping Information

        [StringLength(100), Column("shipping_address")]
        public string? ShippingAddress { get; set; }

        [StringLength(50), Column("shipping_city")]
        public string? ShippingCity { get; set; }

        [StringLength(20), Column("shipping_postal_code")]
        public string? ShippingPostalCode { get; set; }

        [StringLength(50), Column("shipping_country")]
        public string? ShippingCountry { get; set; }

        //Navigation properties

        public virtual ApplicationUser User { get; set; }
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        // Computed properties
        [NotMapped]
        public string FormattedOrderNumber => $"#{OrderNo}";

        [NotMapped]
        public string FormattedTotalAmount => TotalAmount.ToString("C");

        [NotMapped]
        public string FormattedOrderDate => OrderDate.ToString("dd, MM, yyyy");

    }
}