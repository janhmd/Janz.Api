using FluentValidation;
using Service.Helpers.DTOs.Community;

namespace Service.Helpers.Validators.Community
{
    public class PostVoteDtoValidator : AbstractValidator<PostVoteDto>
    {
        public PostVoteDtoValidator()
        {
            RuleFor(m => m.Value)
                .InclusiveBetween(-1, 1).WithMessage("The vote must be 1, -1 or 0.");
        }
    }
}
