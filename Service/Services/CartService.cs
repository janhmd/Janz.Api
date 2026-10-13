using Domain.Entities;
using FluentValidation;
using Repository.Repositories.Interfaces;
using Service.DTOs.Cart;
using Service.DTOs.Common;
using Service.Mappings;
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
                    result.Add(CartMapper.ToDto(item, product));
                }
            }

            return CartMapper.ToDto(result);
        }

        public async Task<ServiceResult<CartDto>> AddAsync(string userId, AddCartItemDto dto)
        {
            var validation = await _addValidator.ValidateAsync(dto);
            if (!validation.IsValid)
            {
                return ServiceResult<CartDto>.Fail(validation.Errors.Select(error => error.ErrorMessage));
            }

            var product = await _productRepo.GetByIdAsync(dto.ProductId);
            if (product is null)
            {
                return ServiceResult<CartDto>.Fail("Product not found.");
            }

            var existing = (await _cartItemRepo.GetAllAsync())
                .FirstOrDefault(m => m.UserId == userId && m.ProductId == dto.ProductId);

            if (existing is null)
            {
                if (dto.Quantity > product.Stock)
                {
                    return ServiceResult<CartDto>.Fail("Not enough stock.");
                }

                await _cartItemRepo.AddAsync(new CartItem
                {
                    UserId = userId,
                    ProductId = dto.ProductId,
                    Quantity = dto.Quantity
                });
            }
            else
            {
                var item = await _cartItemRepo.GetByIdAsync(existing.Id);
                if (item is null)
                {
                    return ServiceResult<CartDto>.Fail("Cart item not found.");
                }

                if (item.Quantity + dto.Quantity > product.Stock)
                {
                    return ServiceResult<CartDto>.Fail("Not enough stock.");
                }

                item.Quantity += dto.Quantity;
                await _cartItemRepo.UpdateAsync(item);
            }

            return ServiceResult<CartDto>.Ok(await GetAsync(userId));
        }

        public async Task<ServiceResult<CartDto>> UpdateAsync(string userId, int itemId, UpdateCartItemDto dto)
        {
            var validation = await _updateValidator.ValidateAsync(dto);
            if (!validation.IsValid)
            {
                return ServiceResult<CartDto>.Fail(validation.Errors.Select(error => error.ErrorMessage));
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

            if (dto.Quantity > product.Stock)
            {
                return ServiceResult<CartDto>.Fail("Not enough stock.");
            }

            item.Quantity = dto.Quantity;
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
    }
}
