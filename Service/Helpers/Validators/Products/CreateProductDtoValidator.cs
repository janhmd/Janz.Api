using FluentValidation;
using Service.Helpers.DTOs.Products;

namespace Service.Helpers.Validators.Products
{
    public class CreateProductDtoValidator : AbstractValidator<CreateProductDto>
    {
        public CreateProductDtoValidator()
        {
            RuleFor(m => m.Name).RequiredText("name", 150);
            RuleFor(m => m.Description).RequiredText("description", 500);

            RuleFor(m => m.Price)
                .GreaterThan(0).WithMessage("The price must be more than 0.")
                .LessThanOrEqualTo(100000).WithMessage("The price can be at most 100 000.");

            RuleFor(m => m.Stock)
                .GreaterThanOrEqualTo(0).WithMessage("The stock can't be negative.")
                .LessThanOrEqualTo(100000).WithMessage("The stock can be at most 100 000.");

            RuleFor(m => m.CategoryId)
                .GreaterThan(0).WithMessage("Choose a category.");

            Include(new ProductDetailsValidator());

            RuleFor(m => m.Gallery)
                .Must(list => list.Count <= ProductRules.MaxGallery)
                .WithMessage($"The gallery can have at most {ProductRules.MaxGallery} images.");
        }
    }
}
