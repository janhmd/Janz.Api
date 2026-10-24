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
        private readonly ICartItemRepository _cartItemRepo;
        private readonly IBaseRepository<Product> _productRepo;
        private readonly IValidator<AddCartItemDto> _addValidator;
        private readonly IValidator<UpdateCartItemDto> _updateValidator;

        public CartService(ICartItemRepository cartItemRepo,
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
            // Məhsul Include ilə birlikdə gəlir: bütün məhsul cədvəlini yükləməyə ehtiyac yoxdur
            var items = await _cartItemRepo.GetByUserAsync(userId);
            var result = items.Select(m => ToDto(m, m.Product)).ToList();

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
            // Gizli məhsul saytda yoxdur: səbətə də əlavə olunmur
            if (product is null || !product.IsVisible)
            {
                return ServiceResult<CartDto>.Fail("Product not found.");
            }

            var item = await _cartItemRepo.GetByProductForUserAsync(model.ProductId, userId);

            if (item is null)
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
                // Eyni məhsul artıq səbətdədir: yeni sətir yox, say artırılır (UserId + ProductId unikal indeksdir)
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

            var item = await _cartItemRepo.GetByIdForUserAsync(itemId, userId);
            if (item is null)
            {
                return ServiceResult<CartDto>.NotFound();
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
            var item = await _cartItemRepo.GetByIdForUserAsync(itemId, userId);
            if (item is null)
            {
                return ServiceResult<CartDto>.NotFound();
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
