using FluentValidation;
using Service.Helpers.DTOs.DiscountLevels;

namespace Service.Helpers.Validators.DiscountLevels
{
    public class SaveDiscountLevelsDtoValidator : AbstractValidator<SaveDiscountLevelsDto>
    {
        public SaveDiscountLevelsDtoValidator()
        {
            RuleFor(m => m.Levels)
                .Must(list => list.Count <= 10).WithMessage("There can be at most 10 levels.")
                .Must(list => list.Select(m => (m.Name ?? "").Trim().ToLower()).Distinct().Count() == list.Count)
                    .WithMessage("Two levels have the same name.")
                .Must(list => list.Select(m => m.MinSpent).Distinct().Count() == list.Count)
                    .WithMessage("Two levels start from the same amount.");

            RuleForEach(m => m.Levels).SetValidator(new SaveDiscountLevelDtoValidator());
        }
    }
}
