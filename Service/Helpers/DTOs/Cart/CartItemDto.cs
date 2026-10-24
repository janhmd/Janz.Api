namespace Service.Helpers.DTOs.Cart
{
    public class CartItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public int Quantity { get; set; }
        public decimal Total { get; set; }
        public int? VariantId { get; set; }
        public string? VariantLabel { get; set; }
        public string? Grind { get; set; }
    }
}
