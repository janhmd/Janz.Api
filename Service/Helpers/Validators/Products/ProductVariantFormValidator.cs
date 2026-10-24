using FluentValidation;
using Service.Helpers.DTOs.Products;

namespace Service.Helpers.Validators.Products
{
    public class ProductVariantFormValidator : AbstractValidator<ProductVariantFormDto>
    {
        public ProductVariantFormValidator()
        {
            RuleFor(m => m.Format).RequiredText("pack type of the variant", 60);
            RuleFor(m => m.Size).OptionalText("size of the variant", 30);

            RuleFor(m => m.Price)
                .GreaterThan(0).WithMessage("The variant price must be more than 0.")
                .LessThanOrEqualTo(100000).WithMessage("The variant price can be at most 100 000.");

            RuleFor(m => m.OldPrice)
                .Must((model, oldPrice) => oldPrice is null || oldPrice > model.Price)
                .WithMessage("The old price of a variant must be more than its price.");

            RuleFor(m => m.Stock)
                .GreaterThanOrEqualTo(0).WithMessage("The variant stock can't be negative.")
                .LessThanOrEqualTo(100000).WithMessage("The variant stock can be at most 100 000.");

            RuleFor(m => m)
                .Must(m => m.KeepImages.Count + m.NewImages.Count <= ProductRules.MaxVariantImages)
                .WithMessage($"A variant can have at most {ProductRules.MaxVariantImages} images.");
        }
    }
}
