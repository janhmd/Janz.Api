using FluentValidation;
using Service.Helpers.DTOs.Pages;

namespace Service.Helpers.Validators.Pages
{
    public class UpdateSitePageDtoValidator : AbstractValidator<UpdateSitePageDto>
    {
        public UpdateSitePageDtoValidator()
        {
            RuleFor(m => m.Title).RequiredText("title", 80);
            RuleFor(m => m.Body).RequiredText("text", 20000);
        }
    }
}
