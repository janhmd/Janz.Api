using FluentValidation;
using Service.Helpers.DTOs.Articles;

namespace Service.Helpers.Validators.Articles
{
    public class ArticleVoteDtoValidator : AbstractValidator<ArticleVoteDto>
    {
        public ArticleVoteDtoValidator()
        {
            RuleFor(m => m.Value)
                .InclusiveBetween(-1, 1).WithMessage("The vote must be 1, -1 or 0.");
        }
    }
}
