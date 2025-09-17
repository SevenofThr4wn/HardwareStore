using System.ComponentModel.DataAnnotations;

namespace HardwareStore.Core.Models
{
    public class ActivityLog
    {
        [Key]
        public int Id { get; set; }
        public string Icon { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}