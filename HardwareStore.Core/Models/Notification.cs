using System.ComponentModel.DataAnnotations;

namespace HardwareStore.Core.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        public string? UserId { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsRead { get; set; } = false;
    }
}
