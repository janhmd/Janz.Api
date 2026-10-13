using FluentValidation;
using Service.DTOs.Cart;

namespace Service.Validations.Cart
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
