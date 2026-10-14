using Domain.Entities;
using FluentValidation;
using Repository.Repositories.Interfaces;
using Service.Helpers.DTOs.Cart;
using Service.Helpers.Responses;
using Service.Services.Interfaces;

namespace Service.Services
{
    public class CartService : ICartService
    {
        private readonly IBaseRepository<CartItem> _cartItemRepo;
        private readonly IBaseRepository<Product> _productRepo;
        private readonly IValidator<AddCartItemDto> _addValidator;
        private readonly IValidator<UpdateCartItemDto> _updateValidator;

        public CartService(IBaseRepository<CartItem> cartItemRepo,
                           IBaseRepository<Product> productRepo,
                           IValidator<AddCartItemDto> addValidator,
                           IValidator<UpdateCartItemDto> updateValidator)
        {
            _cartItemRepo = cartItemRepo;
            _productRepo = productRepo;
            _addValidator = addValidator;
            _updateValidator = updateValidator;
        }

        public async Task<CartDto> GetAsync(string userId)
        {
            var items = (await _cartItemRepo.GetAllAsync()).Where(m => m.UserId == userId);
            var products = (await _productRepo.GetAllAsync()).ToDictionary(m => m.Id);

            var result = new List<CartItemDto>();
            foreach (var item in items)
            {
                if (products.TryGetValue(item.ProductId, out var product))
                {
                    result.Add(ToDto(item, product));
                }
            }

            return ToDto(result);
        }

        public async Task<ServiceResult<CartDto>> AddAsync(string userId, AddCartItemDto model)
        {
            var validation = await _addValidator.ValidateAsync(model);
            if (!validation.IsValid)
            {
                return ServiceResult<CartDto>.Fail(validation.Errors.Select(e => e.ErrorMessage).ToArray());
            }

            var product = await _productRepo.GetByIdAsync(model.ProductId);
            if (product is null)
            {
                return ServiceResult<CartDto>.Fail("Product not found.");
            }

            var existing = (await _cartItemRepo.GetAllAsync())
                .FirstOrDefault(m => m.UserId == userId && m.ProductId == model.ProductId);

            if (existing is null)
            {
                if (model.Quantity > product.Stock)
                {
                    return ServiceResult<CartDto>.Fail("Not enough stock.");
                }

                await _cartItemRepo.AddAsync(new CartItem
                {
                    UserId = userId,
                    ProductId = model.ProductId,
                    Quantity = model.Quantity
                });
            }
            else
            {
                var item = await _cartItemRepo.GetByIdAsync(existing.Id);
                if (item is null)
                {
                    return ServiceResult<CartDto>.Fail("Cart item not found.");
                }

                if (item.Quantity + model.Quantity > product.Stock)
                {
                    return ServiceResult<CartDto>.Fail("Not enough stock.");
                }

                item.Quantity += model.Quantity;
                await _cartItemRepo.UpdateAsync(item);
            }

            return ServiceResult<CartDto>.Ok(await GetAsync(userId));
        }

        public async Task<ServiceResult<CartDto>> UpdateAsync(string userId, int itemId, UpdateCartItemDto model)
        {
            var validation = await _updateValidator.ValidateAsync(model);
            if (!validation.IsValid)
            {
                return ServiceResult<CartDto>.Fail(validation.Errors.Select(e => e.ErrorMessage).ToArray());
            }

            var item = await _cartItemRepo.GetByIdAsync(itemId);
            if (item is null || item.UserId != userId)
            {
                return ServiceResult<CartDto>.Fail("Cart item not found.");
            }

            var product = await _productRepo.GetByIdAsync(item.ProductId);
            if (product is null)
            {
                return ServiceResult<CartDto>.Fail("Product not found.");
            }

            if (model.Quantity > product.Stock)
            {
                return ServiceResult<CartDto>.Fail("Not enough stock.");
            }

            item.Quantity = model.Quantity;
            await _cartItemRepo.UpdateAsync(item);

            return ServiceResult<CartDto>.Ok(await GetAsync(userId));
        }

        public async Task<ServiceResult<CartDto>> RemoveAsync(string userId, int itemId)
        {
            var item = await _cartItemRepo.GetByIdAsync(itemId);
            if (item is null || item.UserId != userId)
            {
                return ServiceResult<CartDto>.Fail("Cart item not found.");
            }

            await _cartItemRepo.DeleteAsync(item);

            return ServiceResult<CartDto>.Ok(await GetAsync(userId));
        }

        private static CartItemDto ToDto(CartItem item, Product product)
        {
            var dto = new CartItemDto();
            Fill(dto, item, product);
            return dto;
        }

        private static void Fill(CartItemDto dto, CartItem item, Product product)
        {
            dto.Id = item.Id;
            dto.ProductId = product.Id;
            dto.ProductName = product.Name;
            dto.ImagePath = product.ImagePath;
            dto.Price = product.Price;
            dto.Stock = product.Stock;
            dto.Quantity = item.Quantity;
            dto.Total = product.Price * item.Quantity;
        }

        private static CartDto ToDto(List<CartItemDto> items)
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
