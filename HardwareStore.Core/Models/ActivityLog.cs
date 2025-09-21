using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HardwareStore.Core.Models
{
    public class ActivityLog
    {
        [Key, Column("activity_log_id")]
        public int Id { get; set; }

        [Required, Column("icon")]
        public string Icon { get; set; } = string.Empty;

        [Required, Column("text")]
        public string Text { get; set; } = string.Empty;

        [Required, Column("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}