using Domain.Entities;
using Janz.Tests.Fakes;
using Service.Helpers.DTOs.Pages;
using Service.Helpers.Exceptions;
using Service.Helpers.Validators.Pages;
using Service.Services;
using Xunit;

namespace Janz.Tests.Services
{
    public class PageServiceTests
    {
        private static (PageService Service, FakeRepository<SitePage> Repo) Create()
        {
            var repo = new FakeRepository<SitePage>();
            repo.Rows.Add(new SitePage { Id = 1, Slug = "delivery", Title = "Delivery and payment", Body = "## Delivery\nFree.", InTerms = true });
            repo.Rows.Add(new SitePage { Id = 2, Slug = "privacy", Title = "Privacy policy", Body = "Text.", InFooter = true });

            return (new PageService(repo, new UpdateSitePageDtoValidator()), repo);
        }

        [Fact]
        public async Task GetLinks_ReturnsAllPagesWithTheirPlaces()
        {
            var (service, _) = Create();

            var links = (await service.GetLinksAsync()).ToList();

            Assert.Equal(2, links.Count);
            Assert.True(links[0].InTerms);
            Assert.True(links[1].InFooter);
        }

        [Fact]
        public async Task GetBySlug_ReturnsThePage()
        {
            var (service, _) = Create();

            var page = await service.GetBySlugAsync("delivery");

            Assert.Equal("Delivery and payment", page.Title);
        }

        [Fact]
        public async Task GetBySlug_ThrowsNotFound_ForUnknownSlug()
        {
            var (service, _) = Create();

            await Assert.ThrowsAsync<NotFoundException>(() => service.GetBySlugAsync("nope"));
        }

        [Fact]
        public async Task Update_ChangesTextAndPlaces_AndTrimsInput()
        {
            var (service, repo) = Create();

            await service.UpdateAsync(1, new UpdateSitePageDto { Title = "  Delivery  ", Body = "  New text  ", InTerms = false, InFooter = true });

            var page = repo.Rows.First(m => m.Id == 1);
            Assert.Equal("Delivery", page.Title);
            Assert.Equal("New text", page.Body);
            Assert.False(page.InTerms);
            Assert.True(page.InFooter);
        }

        [Fact]
        public async Task Update_ThrowsBadRequest_WhenTitleIsEmpty()
        {
            var (service, _) = Create();

            await Assert.ThrowsAsync<BadRequestException>(() => service.UpdateAsync(1, new UpdateSitePageDto { Title = " ", Body = "Text" }));
        }

        [Fact]
        public async Task Update_ThrowsNotFound_ForUnknownId()
        {
            var (service, _) = Create();

            await Assert.ThrowsAsync<NotFoundException>(() => service.UpdateAsync(99, new UpdateSitePageDto { Title = "Title", Body = "Text" }));
        }
    }
}
