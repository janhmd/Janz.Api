using FluentValidation;
using Service.Helpers.DTOs.Reviews;

namespace Service.Helpers.Validators.Reviews
{
    public class SaveShopReviewDtoValidator : AbstractValidator<SaveShopReviewDto>
    {
        public SaveShopReviewDtoValidator()
        {
            RuleFor(m => m.ShopRating).InclusiveBetween(1, 5).WithMessage("Rate the shop from 1 to 5 stars.");
            RuleFor(m => m.DeliveryRating).InclusiveBetween(1, 5).WithMessage("Rate the delivery from 1 to 5 stars.");
            RuleFor(m => m.ShopText).OptionalText("shop text", 500);
            RuleFor(m => m.DeliveryText).OptionalText("delivery text", 500);
            RuleFor(m => m.Comment).OptionalText("comment", 1000);
        }
    }
}
