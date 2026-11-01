using FluentValidation;
using Service.Helpers.DTOs.DiscountLevels;

namespace Service.Helpers.Validators.DiscountLevels
{
    public class SaveDiscountLevelDtoValidator : AbstractValidator<SaveDiscountLevelDto>
    {
        public SaveDiscountLevelDtoValidator()
        {
            RuleFor(m => m.Name).RequiredText("level name", 50);
            RuleFor(m => m.Perks).OptionalText("perks", 200);

            RuleFor(m => m.MinSpent)
                .GreaterThanOrEqualTo(0).WithMessage("The spent amount can't be negative.")
                .LessThanOrEqualTo(1000000).WithMessage("The spent amount can be at most 1 000 000.");

            RuleFor(m => m.Percent)
                .InclusiveBetween(0, 50).WithMessage("The discount must be from 0 to 50%.");
        }
    }
}
