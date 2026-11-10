using Domain.Entities;
using Janz.Tests.Fakes;
using Service.Helpers.DTOs.GiftCertificates;
using Service.Helpers.DTOs.Users;
using Service.Helpers.Exceptions;
using Service.Helpers.Validators.GiftCertificates;
using Service.Services;
using Xunit;

namespace Janz.Tests.Services
{
    public class GiftCertificateNotifyTests
    {
        private readonly FakeRepository<GiftCertificate> _certificates = new();
        private readonly FakeUserAdminService _users = new();
        private readonly FakeNotificationService _notifications = new();

        public GiftCertificateNotifyTests()
        {
            _users.Users.Add(new UserAdminDto { Id = "u1", Role = "User" });
            _users.Users.Add(new UserAdminDto { Id = "u2", Role = "User", IsBlocked = true });
            _users.Users.Add(new UserAdminDto { Id = "staff", Role = "Admin" });
            _certificates.Rows.Add(new GiftCertificate { Id = 1, Code = "GIFT50", Amount = 50, IsActive = true });
        }

        // Bildiriş göndərişi UserManager-dən istifadə etmir
        private GiftCertificateService Create() => new(_certificates, null!, _users, _notifications, new CreateGiftCertificateDtoValidator(), new UpdateGiftCertificateDtoValidator());

        [Fact]
        public async Task All_GoesToUnblockedCustomersOnly()
        {
            var sent = await Create().NotifyAsync(1, new NotifyGiftCertificateDto { Audience = "All" });

            Assert.Equal(1, sent);
            var added = Assert.Single(_notifications.Added);
            Assert.Equal("u1", added.UserId);
            Assert.Contains("GIFT50", added.Text);
            Assert.Contains("₼50.00", added.Text);
            Assert.Equal("/Profile#ledger", added.Link);
        }

        [Fact]
        public async Task Selected_SkipsStaffAndUnknown()
        {
            var sent = await Create().NotifyAsync(1, new NotifyGiftCertificateDto { Audience = "Selected", UserIds = new List<string> { "staff", "nobody" } }.WithExtra("u1"));

            Assert.Equal(1, sent);
        }

        [Fact]
        public async Task UsedDisabledOrExpired_IsNotSent()
        {
            _certificates.Rows[0].UsedById = "u1";
            await Assert.ThrowsAsync<BadRequestException>(() => Create().NotifyAsync(1, new NotifyGiftCertificateDto { Audience = "All" }));

            _certificates.Rows[0].UsedById = null;
            _certificates.Rows[0].IsActive = false;
            await Assert.ThrowsAsync<BadRequestException>(() => Create().NotifyAsync(1, new NotifyGiftCertificateDto { Audience = "All" }));

            _certificates.Rows[0].IsActive = true;
            _certificates.Rows[0].ExpiresAt = DateTime.UtcNow.AddDays(-1);
            await Assert.ThrowsAsync<BadRequestException>(() => Create().NotifyAsync(1, new NotifyGiftCertificateDto { Audience = "All" }));
            Assert.Empty(_notifications.Added);
        }

        [Fact]
        public async Task NobodyChosen_OrMissing_IsRejected()
        {
            await Assert.ThrowsAsync<BadRequestException>(() => Create().NotifyAsync(1, new NotifyGiftCertificateDto { Audience = "Selected", UserIds = new List<string>() }));
            await Assert.ThrowsAsync<NotFoundException>(() => Create().NotifyAsync(9, new NotifyGiftCertificateDto { Audience = "All" }));
        }
    }

    internal static class NotifyDtoExtensions
    {
        public static NotifyGiftCertificateDto WithExtra(this NotifyGiftCertificateDto dto, string userId)
        {
            dto.UserIds!.Add(userId);
            return dto;
        }
    }
}
