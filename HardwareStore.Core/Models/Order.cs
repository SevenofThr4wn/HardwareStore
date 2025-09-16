using HardwareStore.Core.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HardwareStore.Core.Models
{
    public class Order
    {
        [Key, Column("order_id")]
        public int Id { get; set; }

        [Column("user_id")]
        public string UserId { get; set; } = string.Empty;
        public User User { get; set; } = null!;

        [Column("order_date")]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Column("status")]
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}