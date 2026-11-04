using FluentValidation;
using Service.Helpers.DTOs.Community;

namespace Service.Helpers.Validators.Community
{
    public class UpdateCommunityProfileDtoValidator : AbstractValidator<UpdateCommunityProfileDto>
    {
        public UpdateCommunityProfileDtoValidator()
        {
            RuleFor(m => m.Bio)
                .MaximumLength(180).WithMessage("The bio can be at most 180 characters.");

            RuleFor(m => m.CookMethods)
                .Must(list => list.Count <= 12).WithMessage("Choose at most 12 brewing methods.");

            RuleForEach(m => m.CookMethods)
                .NotEmpty().WithMessage("A brewing method can't be empty.")
                .MaximumLength(40).WithMessage("A brewing method can be at most 40 characters.");
        }
    }
}
