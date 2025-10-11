using HardwareStore.Core.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HardwareStore.Core.Models
{
    [Table("Orders")]
    public class Order
    {
        [Key]
        [Column("OrderId")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Column("OrderNumber")]
        public string OrderNo { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Required]
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        [Required]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedDate { get; set; }

        // Shipping Information

        [StringLength(100)]
        public string? ShippingAddress { get; set; }

        [StringLength(50)]
        public string? ShippingCity { get; set; }

        [StringLength(20)]
        public string? ShippingPostalCode { get; set; }

        [StringLength(50)]
        public string? ShippingCountry { get; set; }

        //Navigation properties

        public virtual ApplicationUser? User { get; set; }
        public virtual List<OrderItem> OrderItems { get; set; } = new();

        // Computed properties

        [NotMapped]
        public string FormattedOrderNumber => $"#{OrderNo}";

        [NotMapped]
        public string FullShippingAddress => $"{ShippingAddress}, {ShippingCity}, {ShippingPostalCode}, {ShippingCountry}";
    }
}