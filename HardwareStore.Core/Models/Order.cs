using HardwareStore.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace HardwareStore.Core.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;
        public User User { get; set; } = null!;

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        public ICollection<OrderItem> OrderItems { get; set; }
    }
}