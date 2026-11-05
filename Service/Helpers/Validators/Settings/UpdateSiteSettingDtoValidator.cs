using FluentValidation;
using Service.Helpers.DTOs.Settings;

namespace Service.Helpers.Validators.Settings
{
    public class UpdateSiteSettingDtoValidator : AbstractValidator<UpdateSiteSettingDto>
    {
        public UpdateSiteSettingDtoValidator()
        {
            RuleFor(m => m.Phone).RequiredText("phone", 30);
            RuleFor(m => m.Email).RequiredText("email", 100)
                .EmailAddress().WithMessage("Enter a valid email.");
            RuleFor(m => m.Address).RequiredText("address", 150);
            RuleFor(m => m.Instagram).RequiredText("Instagram link", 200);
            RuleFor(m => m.WhatsApp).RequiredText("WhatsApp link", 200);
            RuleFor(m => m.Hours).RequiredText("working hours", 100);
        }
    }
}
