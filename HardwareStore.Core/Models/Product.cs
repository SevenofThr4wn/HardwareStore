using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HardwareStore.Core.Models
{
    public class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProductId { get; set; }

        [Required]
        [StringLength(100)]
        [Column("ProductName")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        [Column("ProductDescription")]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Column("ProductPrice", TypeName = "decimal(18,2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }

        [Required]
        [StringLength(100)]
        public string Category { get; set; } = string.Empty;

        [StringLength(500)]
        public string? ImageUrl { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? Updated { get; set; }

        // Computed Properties

        [NotMapped]
        public bool IsInStock => StockQuantity > 0;

        [NotMapped]
        public bool IsLowStock => StockQuantity > 0 && StockQuantity <= 10;

    }
}