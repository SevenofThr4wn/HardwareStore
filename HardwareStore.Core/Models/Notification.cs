using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HardwareStore.Core.Models
{
    [Table("Notifications")]
    public class Notification
    {
        [Key]
        [Column("NotificationId")]
        public int Id { get; set; }

        [Column("UserId")]
        public string? UserId { get; set; }
        public string? Title { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;
    }
}