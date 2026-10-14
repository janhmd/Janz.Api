namespace Service.Helpers.DTOs.Cart
{
    public class CartDto
    {
        public List<CartItemDto> Items { get; set; } = new List<CartItemDto>();
        public int Count { get; set; }
        public decimal Total { get; set; }
    }
}
