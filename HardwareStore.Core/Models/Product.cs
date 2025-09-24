using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HardwareStore.Core.Models
{
    public class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProductId { get; set; }

        [Required, Column("product_name"), StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, Column("product_description"), StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Column("product_price", TypeName = "decimal(18,2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        [Required, Column("stock_quantity"), Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }

        [Required, Column("category"), StringLength(100)]
        public string Category { get; set; } = string.Empty;

        [Column("image_url"), StringLength(500)]
        public string? ImageUrl { get; set; }

        [Required, Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [Column("date_updated")]
        public DateTime? Updated { get; set; }

        // Computed Properties

        [NotMapped]
        public bool IsInStock => StockQuantity > 0;

        [NotMapped]
        public bool IsLowStock => StockQuantity > 0 && StockQuantity <= 10;

        // Methods
        public void UpdateStock(int quantity)
        {
            if (StockQuantity + quantity < 0)
                throw new InvalidOperationException("Insufficient stock");

            StockQuantity += quantity;
            Updated = DateTime.UtcNow;
        }
    }
}