using Service.Helpers.DTOs.Pages;
using Service.Helpers.Validators.Pages;
using Xunit;

namespace Janz.Tests.Validators
{
    public class PageValidatorTests
    {
        private readonly UpdateSitePageDtoValidator _validator = new();

        [Fact]
        public void Valid_Page_Passes()
        {
            var result = _validator.Validate(new UpdateSitePageDto { Title = "FAQ", Body = "## Q\nA" });

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Title_Over80Characters_Fails()
        {
            var result = _validator.Validate(new UpdateSitePageDto { Title = new string('a', 81), Body = "Text" });

            Assert.False(result.IsValid);
        }

        [Fact]
        public void Empty_Body_Fails()
        {
            var result = _validator.Validate(new UpdateSitePageDto { Title = "FAQ", Body = "" });

            Assert.False(result.IsValid);
        }
    }
}
