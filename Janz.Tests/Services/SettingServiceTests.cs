using Domain.Entities;
using Janz.Tests.Fakes;
using Service.Helpers.DTOs.Settings;
using Service.Helpers.Exceptions;
using Service.Helpers.Validators.Settings;
using Service.Services;
using Xunit;

namespace Janz.Tests.Services
{
    public class SettingServiceTests
    {
        private static (SettingService Service, FakeRepository<SiteSetting> Repo) Create(bool withRow = true)
        {
            var repo = new FakeRepository<SiteSetting>();
            if (withRow)
            {
                repo.Rows.Add(new SiteSetting { Id = 1, Phone = "+994 70 217 55 57", Email = "a@janz.az", Address = "Baku", Instagram = "https://instagram.com/janz", WhatsApp = "https://wa.me/1", Hours = "10:00-20:00" });
            }

            return (new SettingService(repo, new UpdateSiteSettingDtoValidator()), repo);
        }

        private static UpdateSiteSettingDto Valid() => new()
        {
            Phone = " +994 55 000 00 00 ",
            Email = "new@janz.az",
            Address = "Nesimi",
            Instagram = "https://instagram.com/new",
            WhatsApp = "https://wa.me/2",
            Hours = "Every day"
        };

        [Fact]
        public async Task Get_ReturnsContacts()
        {
            var (service, _) = Create();

            var setting = await service.GetAsync();

            Assert.Equal("+994 70 217 55 57", setting.Phone);
        }

        [Fact]
        public async Task Get_ThrowsNotFound_WhenNothingIsSaved()
        {
            var (service, _) = Create(false);

            await Assert.ThrowsAsync<NotFoundException>(() => service.GetAsync());
        }

        [Fact]
        public async Task Update_SavesTrimmedValues()
        {
            var (service, repo) = Create();

            await service.UpdateAsync(Valid());

            Assert.Equal("+994 55 000 00 00", repo.Rows[0].Phone);
            Assert.Equal("new@janz.az", repo.Rows[0].Email);
        }

        [Fact]
        public async Task Update_ThrowsBadRequest_ForInvalidEmail()
        {
            var (service, repo) = Create();
            var model = Valid();
            model.Email = "not-an-email";

            await Assert.ThrowsAsync<BadRequestException>(() => service.UpdateAsync(model));
            Assert.Equal("a@janz.az", repo.Rows[0].Email);
        }

        [Fact]
        public async Task Update_ThrowsBadRequest_WhenPhoneIsMissing()
        {
            var (service, _) = Create();
            var model = Valid();
            model.Phone = null;

            await Assert.ThrowsAsync<BadRequestException>(() => service.UpdateAsync(model));
        }
    }
}
