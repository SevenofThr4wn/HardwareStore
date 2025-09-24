namespace HardwareStore.WebClient.ViewModels.Product.Edit
{
    public class ProductEditVM
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string Category { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
