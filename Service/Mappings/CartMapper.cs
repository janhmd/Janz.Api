using Domain.Entities;
using Service.DTOs.Cart;

namespace Service.Mappings
{
    public static class CartMapper
    {
        public static CartItemDto ToDto(CartItem item, Product product)
        {
            return new CartItemDto
            {
                Id = item.Id,
                ProductId = product.Id,
                ProductName = product.Name,
                ImagePath = product.ImagePath,
                Price = product.Price,
                Stock = product.Stock,
                Quantity = item.Quantity,
                Total = product.Price * item.Quantity
            };
        }

        public static CartDto ToDto(List<CartItemDto> items)
        {
            return new CartDto
            {
                Items = items,
                Count = items.Sum(m => m.Quantity),
                Total = items.Sum(m => m.Total)
            };
        }
    }
}
