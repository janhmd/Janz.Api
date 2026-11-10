using Domain.Entities;
using Domain.Enums;
using Janz.Tests.Fakes;
using Service.Helpers.DTOs.Coupons;
using Service.Helpers.DTOs.Users;
using Service.Helpers.Exceptions;
using Service.Helpers.Validators.Coupons;
using Service.Services;
using Xunit;

namespace Janz.Tests.Services
{
    public class CouponNotifyTests
    {
        private readonly FakeRepository<Coupon> _coupons = new();
        private readonly FakeRepository<Order> _orders = new();
        private readonly FakeUserAdminService _users = new();
        private readonly FakeNotificationService _notifications = new();

        public CouponNotifyTests()
        {
            _users.Users.Add(new UserAdminDto { Id = "u1", Role = "User" });
            _users.Users.Add(new UserAdminDto { Id = "u2", Role = "User" });
            _users.Users.Add(new UserAdminDto { Id = "u3", Role = "User", IsBlocked = true });
            _users.Users.Add(new UserAdminDto { Id = "staff", Role = "Admin" });
            _coupons.Rows.Add(new Coupon { Id = 1, Code = "WELCOME10", Type = CouponType.Percent, Value = 10, MinOrder = 30, IsActive = true });
        }

        private CouponService Create() => new(_coupons, _orders, _users, _notifications, new CreateCouponDtoValidator(), new UpdateCouponDtoValidator());

        [Fact]
        public async Task All_GoesToUnblockedCustomersOnly()
        {
            var sent = await Create().NotifyAsync(1, new NotifyCouponDto { Audience = "All" });

            Assert.Equal(2, sent);
            Assert.Equal(new[] { "u1", "u2" }, _notifications.Added.Select(m => m.UserId).OrderBy(m => m).ToArray());
            var first = _notifications.Added[0];
            Assert.Contains("WELCOME10", first.Text);
            Assert.Contains("10% off", first.Text);
            Assert.Contains("₼30.00", first.Text);
            Assert.Equal("/Cart", first.Link);
        }

        [Fact]
        public async Task Selected_SkipsStaffBlockedUnknownAndDuplicates()
        {
            var sent = await Create().NotifyAsync(1, new NotifyCouponDto { Audience = "selected", UserIds = new List<string> { "u1", "u1", "staff", "u3", "nobody" } });

            Assert.Equal(1, sent);
            Assert.Equal("u1", Assert.Single(_notifications.Added).UserId);
        }

        [Fact]
        public async Task Selected_WithNobody_IsRejected()
        {
            await Assert.ThrowsAsync<BadRequestException>(() => Create().NotifyAsync(1, new NotifyCouponDto { Audience = "Selected", UserIds = new List<string>() }));
            Assert.Empty(_notifications.Added);
        }

        [Fact]
        public async Task UnknownAudience_IsRejected()
        {
            await Assert.ThrowsAsync<BadRequestException>(() => Create().NotifyAsync(1, new NotifyCouponDto { Audience = "Everyone" }));
        }

        [Fact]
        public async Task DisabledOrExpiredCoupon_IsNotSent()
        {
            _coupons.Rows[0].IsActive = false;
            await Assert.ThrowsAsync<BadRequestException>(() => Create().NotifyAsync(1, new NotifyCouponDto { Audience = "All" }));

            _coupons.Rows[0].IsActive = true;
            _coupons.Rows[0].EndsAt = DateTime.UtcNow.AddDays(-2);
            await Assert.ThrowsAsync<BadRequestException>(() => Create().NotifyAsync(1, new NotifyCouponDto { Audience = "All" }));
        }

        [Fact]
        public async Task MissingCoupon_IsNotFound()
        {
            await Assert.ThrowsAsync<NotFoundException>(() => Create().NotifyAsync(99, new NotifyCouponDto { Audience = "All" }));
        }
    }
}
