using Domain.Constants;
using FluentValidation;
using Service.Helpers.DTOs.Tiles;

namespace Service.Helpers.Validators.Tiles
{
    public class SaveSiteTileDtoValidator : AbstractValidator<SaveSiteTileDto>
    {
        public SaveSiteTileDtoValidator()
        {
            RuleFor(m => m.Placement)
                .Must(value => value is not null && TilePlacements.All.Contains(value))
                .WithMessage("Choose where the tile is shown.");

            RuleFor(m => m.Title).RequiredText("title", 40);

            // Yalnız saytın öz ünvanı: "/Products/Tea" və ya filtr "?tag=oolong". Kənar sayt və "javascript:" olmaz
            RuleFor(m => m.Link)
                .Must(value => !string.IsNullOrWhiteSpace(value)).WithMessage("Enter the link.")
                .Must(value => string.IsNullOrWhiteSpace(value) || value.Trim().Length <= 150).WithMessage("The link can be at most 150 characters.")
                .Must(value => string.IsNullOrWhiteSpace(value) || IsInternal(value.Trim()))
                .WithMessage("The link must stay on the site: start it with / or ?, for example /Products/Tea or ?tag=oolong.");

            RuleFor(m => m.SortOrder)
                .InclusiveBetween(0, 999).WithMessage("The order must be between 0 and 999.");
        }

        private static bool IsInternal(string link)
        {
            if (link.Any(char.IsWhiteSpace) || link.StartsWith("//") || link.Contains('\\'))
            {
                return false;
            }

            return link.StartsWith('/') || link.StartsWith('?');
        }
    }
}
