using Domain.Constants;
using FluentValidation;
using Service.Helpers.DTOs.Products;

namespace Service.Helpers.Validators.Products
{
    // Create və Update validatorları bunu Include ilə istifadə edir
    public class ProductDetailsValidator : AbstractValidator<IProductDetails>
    {
        public ProductDetailsValidator()
        {
            // Köhnə qiymət yalnız endirim varsa mənalıdır: indiki qiymətdən böyük olmalıdır
            RuleFor(m => m.OldPrice)
                .Must((model, oldPrice) => oldPrice is null || oldPrice > model.Price)
                .WithMessage("The old price must be more than the price.")
                .LessThanOrEqualTo(100000).WithMessage("The old price can be at most 100 000.")
                .When(m => !m.HasVariants);

            RuleFor(m => m.Badge)
                .Must(badge => string.IsNullOrWhiteSpace(badge) || ProductBadges.All.Contains(badge.Trim()))
                .WithMessage("Choose a badge from the list.");

            RuleFor(m => m.Roast).OptionalText("roast level", 50);
            RuleFor(m => m.Region).OptionalText("region", 50);
            RuleFor(m => m.Processing).OptionalText("processing", 50);
            RuleFor(m => m.Caffeine).OptionalText("caffeine", 20);
            RuleFor(m => m.Brewing).OptionalText("brewing text", 1000);
            RuleFor(m => m.Storage).OptionalText("storage text", 500);

            RuleFor(m => m.BrewMethods)
                .Must(list => list.All(m => BrewMethods.All.Contains(m)))
                .WithMessage("Choose brewing methods from the list.");

            RuleFor(m => m.Variants)
                .Must(list => list.Count <= ProductRules.MaxVariants)
                .WithMessage($"A product can have at most {ProductRules.MaxVariants} variants.")
                // Eyni paket və ölçü iki dəfə ola bilməz (məs. iki dəfə "Pack · 250 g")
                .Must(list => list.Select(m => ((m.Format ?? "").Trim().ToLower(), (m.Size ?? "").Trim().ToLower())).Distinct().Count() == list.Count)
                .WithMessage("Two variants have the same pack and size.");

            RuleForEach(m => m.Variants).SetValidator(new ProductVariantFormValidator());
        }
    }
}
