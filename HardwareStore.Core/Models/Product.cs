using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HardwareStore.Core.Models
{
    public class Product
    {
        [Key, Column("product_id")]
        public int ProductId { get; set; }

        [Required, MaxLength(200), Column("product_name")]
        public string Name { get; set; } = string.Empty;

        [Required, Column("product_price")]
        public double Price { get; set; }

        [Column("product_stock")]
        public int Stock { get; set; }
    }
}