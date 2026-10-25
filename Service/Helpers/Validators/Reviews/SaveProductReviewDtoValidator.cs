using FluentValidation;
using Service.Helpers.DTOs.Reviews;

namespace Service.Helpers.Validators.Reviews
{
    public class SaveProductReviewDtoValidator : AbstractValidator<SaveProductReviewDto>
    {
        public SaveProductReviewDtoValidator()
        {
            RuleFor(m => m.Rating)
                .InclusiveBetween(1, 5).WithMessage("Choose from 1 to 5 stars.");

            RuleFor(m => m.Text).OptionalText("review", 1000);
        }
    }
}
