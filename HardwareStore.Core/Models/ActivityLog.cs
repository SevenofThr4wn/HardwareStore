using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HardwareStore.Core.Models
{
    [Table("ActivityLogs")]
    [Index(nameof(Icon), nameof(Timestamp), IsUnique = true)]
    public class ActivityLog
    {
        [Key]
        [Column("ActivityLogId")]
        public int Id { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "Icona name cannot exceed 50 characters.")]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Icon can only contain letters, numbers, and underscores.")]
        public string Icon { get; set; } = string.Empty;

        [Required]
        [StringLength(500, ErrorMessage = "Text cannot exceed 500 characters.")]
        public string Text { get; set; } = string.Empty;

        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [CustomValidation(typeof(ActivityLog), nameof(ValidateTimestamp))]
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Validates that the timestamp is not set in the future.
        /// </summary>
        /// <param name="timestamp">The current timestamp.</param>
        /// <param name="context">The validation context to use.</param>
        /// <returns>Whether the timestamp has been validated sucessfully or not.</returns>
        public static ValidationResult? ValidateTimestamp(DateTime timestamp, ValidationContext context)
        {
            if (timestamp > DateTime.UtcNow)
                return new ValidationResult("Timestamp cannot be in the future.");
            return ValidationResult.Success;
        }
    }
}