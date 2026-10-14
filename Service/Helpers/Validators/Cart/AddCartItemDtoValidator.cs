using FluentValidation;
using Service.Helpers.DTOs.Cart;

namespace Service.Helpers.Validators.Cart
{
    public class AddCartItemDtoValidator : AbstractValidator<AddCartItemDto>
    {
        public AddCartItemDtoValidator()
        {
            RuleFor(m => m.ProductId)
                .GreaterThan(0).WithMessage("Product is required.");

            RuleFor(m => m.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be at least 1.");
        }
    }
}
