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
        private readonly IBaseRepository<ProductVariant> _variantRepo;
        private readonly IValidator<AddCartItemDto> _addValidator;
        private readonly IValidator<UpdateCartItemDto> _updateValidator;

        public CartService(ICartItemRepository cartItemRepo,
                           IBaseRepository<Product> productRepo,
                           IBaseRepository<ProductVariant> variantRepo,
                           IValidator<AddCartItemDto> addValidator,
                           IValidator<UpdateCartItemDto> updateValidator)
        {
            _cartItemRepo = cartItemRepo;
            _productRepo = productRepo;
            _variantRepo = variantRepo;
            _addValidator = addValidator;
            _updateValidator = updateValidator;
        }

        public async Task<CartDto> GetAsync(string userId)
        {
            // Məhsul Include ilə birlikdə gəlir: bütün məhsul cədvəlini yükləməyə ehtiyac yoxdur
            var items = await _cartItemRepo.GetByUserAsync(userId);
            var result = items.Select(m => ToDto(m, m.Product, m.Variant)).ToList();

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

            // Variantı olan məhsulda variant seçilməlidir və bu məhsula aid olmalıdır
            var variants = (await _variantRepo.GetAllAsync()).Where(m => m.ProductId == product.Id).ToList();
            ProductVariant? variant = null;
            if (variants.Count > 0)
            {
                variant = variants.FirstOrDefault(m => m.Id == model.VariantId);
                if (variant is null)
                {
                    return ServiceResult<CartDto>.Fail("Choose a variant.");
                }
            }

            var grind = string.IsNullOrWhiteSpace(model.Grind) ? null : model.Grind.Trim();

            // Stok variant üzrədir: eyni variantın fərqli üyütmə ilə sətirləri də sayılır
            var stock = variant?.Stock ?? product.Stock;
            var inCart = (await _cartItemRepo.GetByUserAsync(userId))
                .Where(m => m.ProductId == product.Id && m.VariantId == variant?.Id)
                .Sum(m => m.Quantity);
            if (inCart + model.Quantity > stock)
            {
                return ServiceResult<CartDto>.Fail("Not enough stock.");
            }

            var item = await _cartItemRepo.GetLineForUserAsync(product.Id, variant?.Id, grind, userId);
            if (item is null)
            {
                await _cartItemRepo.AddAsync(new CartItem
                {
                    UserId = userId,
                    ProductId = product.Id,
                    VariantId = variant?.Id,
                    Grind = grind,
                    Quantity = model.Quantity
                });
            }
            else
            {
                // Eyni məhsul, variant və üyütmə artıq səbətdədir: yeni sətir yox, say artırılır
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

            var variant = item.VariantId is int variantId ? await _variantRepo.GetByIdAsync(variantId) : null;
            var stock = variant?.Stock ?? product.Stock;
            var otherLines = (await _cartItemRepo.GetByUserAsync(userId))
                .Where(m => m.Id != item.Id && m.ProductId == item.ProductId && m.VariantId == item.VariantId)
                .Sum(m => m.Quantity);

            if (otherLines + model.Quantity > stock)
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

        private static CartItemDto ToDto(CartItem item, Product product, ProductVariant? variant)
        {
            var dto = new CartItemDto();
            Fill(dto, item, product, variant);
            return dto;
        }

        // Variant varsa qiymət, stok və şəkil variantdan götürülür
        private static void Fill(CartItemDto dto, CartItem item, Product product, ProductVariant? variant)
        {
            var price = variant?.Price ?? product.Price;
            dto.Id = item.Id;
            dto.ProductId = product.Id;
            dto.ProductName = product.Name;
            dto.ImagePath = variant?.Images.OrderBy(m => m.Order).Select(m => m.ImagePath).FirstOrDefault() ?? product.ImagePath;
            dto.Price = price;
            dto.Stock = variant?.Stock ?? product.Stock;
            dto.Quantity = item.Quantity;
            dto.Total = price * item.Quantity;
            dto.VariantId = variant?.Id;
            dto.VariantLabel = variant?.Label;
            dto.Grind = item.Grind;
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
