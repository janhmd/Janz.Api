using FluentValidation;
using Service.Helpers.DTOs.Community;

namespace Service.Helpers.Validators.Community
{
    public class ReportPostDtoValidator : AbstractValidator<ReportPostDto>
    {
        public ReportPostDtoValidator()
        {
            RuleFor(m => m.Reason).RequiredText("reason", 200);
        }
    }
}
