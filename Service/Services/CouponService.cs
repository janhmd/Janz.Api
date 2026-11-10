using Domain.Constants;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using Repository.Repositories.Interfaces;
using Service.Helpers.DTOs.Coupons;
using Service.Helpers.Exceptions;
using Service.Helpers.Validators;
using Service.Services.Interfaces;

namespace Service.Services
{
    public class CouponService : ICouponService
    {
        private readonly IBaseRepository<Coupon> _couponRepo;
        private readonly IBaseRepository<Order> _orderRepo;
        private readonly IUserAdminService _userService;
        private readonly INotificationService _notificationService;
        private readonly IValidator<CreateCouponDto> _createValidator;
        private readonly IValidator<UpdateCouponDto> _updateValidator;

        public CouponService(IBaseRepository<Coupon> couponRepo,
                             IBaseRepository<Order> orderRepo,
                             IUserAdminService userService,
                             INotificationService notificationService,
                             IValidator<CreateCouponDto> createValidator,
                             IValidator<UpdateCouponDto> updateValidator)
        {
            _couponRepo = couponRepo;
            _orderRepo = orderRepo;
            _userService = userService;
            _notificationService = notificationService;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        public async Task<IEnumerable<CouponDto>> GetAllAsync(string? status)
        {
            var coupons = (await _couponRepo.GetAllAsync()).Select(ToDto);

            if (!string.IsNullOrWhiteSpace(status))
            {
                coupons = coupons.Where(m => string.Equals(m.Status, status.Trim(), StringComparison.OrdinalIgnoreCase));
            }

            return coupons.ToList();
        }

        public async Task<CouponDto> GetByIdAsync(int id)
        {
            var coupon = await _couponRepo.GetByIdAsync(id);
            if (coupon is null)
            {
                throw new NotFoundException();
            }

            return ToDto(coupon);
        }

        public async Task<CouponDto> CreateAsync(CreateCouponDto model)
        {
            await _createValidator.EnsureValidAsync(model);

            var code = model.Code!.Trim().ToUpperInvariant();
            await EnsureCodeIsFreeAsync(code, null);

            var coupon = new Coupon
            {
                Code = code,
                Type = Enum.Parse<CouponType>(model.Type!, true),
                Value = model.Value,
                MinOrder = model.MinOrder,
                UsageLimit = model.UsageLimit,
                StartsAt = model.StartsAt?.Date,
                EndsAt = model.EndsAt?.Date,
                IsActive = model.IsActive,
                IsFirstOrder = model.IsFirstOrder
            };
            await _couponRepo.AddAsync(coupon);

            return ToDto(coupon);
        }

        public async Task UpdateAsync(int id, UpdateCouponDto model)
        {
            await _updateValidator.EnsureValidAsync(model);

            var coupon = await _couponRepo.GetByIdAsync(id);
            if (coupon is null)
            {
                throw new NotFoundException();
            }

            var code = model.Code!.Trim().ToUpperInvariant();
            await EnsureCodeIsFreeAsync(code, id);

            coupon.Code = code;
            coupon.Type = Enum.Parse<CouponType>(model.Type!, true);
            coupon.Value = model.Value;
            coupon.MinOrder = model.MinOrder;
            coupon.UsageLimit = model.UsageLimit;
            coupon.StartsAt = model.StartsAt?.Date;
            coupon.EndsAt = model.EndsAt?.Date;
            coupon.IsActive = model.IsActive;
            coupon.IsFirstOrder = model.IsFirstOrder;
            await _couponRepo.UpdateAsync(coupon);
        }

        public async Task DeleteAsync(int id)
        {
            var coupon = await _couponRepo.GetByIdAsync(id);
            if (coupon is null)
            {
                throw new NotFoundException();
            }

            await _couponRepo.DeleteAsync(coupon);
        }

        public async Task<int> NotifyAsync(int id, NotifyCouponDto model)
        {
            var coupon = await _couponRepo.GetByIdAsync(id);
            if (coupon is null)
            {
                throw new NotFoundException();
            }

            var status = StatusOf(coupon);
            if (status != "Active" && status != "Scheduled")
            {
                throw new BadRequestException($"This coupon can't be sent now: it is {status.ToLowerInvariant()}.");
            }

            var all = string.Equals(model.Audience, "All", StringComparison.OrdinalIgnoreCase);
            var selected = string.Equals(model.Audience, "Selected", StringComparison.OrdinalIgnoreCase);
            if (!all && !selected)
            {
                throw new BadRequestException("Choose who gets the coupon.");
            }

            // Yalnız bloklanmamış müştərilər; seçilmişlərdə bilinməyən və ya işçi hesabları atılır
            var customers = (await _userService.GetAllAsync(Roles.User, false, null)).Select(m => m.Id).ToHashSet();
            var recipients = all
                ? customers.ToList()
                : (model.UserIds ?? new List<string>()).Where(customers.Contains).Distinct().ToList();
            if (recipients.Count == 0)
            {
                throw new BadRequestException("Choose at least one customer.");
            }

            var text = $"Use the code {coupon.Code} at checkout: {Describe(coupon)}.";
            foreach (var userId in recipients)
            {
                await _notificationService.AddAsync(userId, NotificationKind.Shop, "A promo code for you.", text, "/Cart");
            }

            return recipients.Count;
        }

        // "10% off an order from ₼30, until 31.12.2026"
        private static string Describe(Coupon coupon)
        {
            var text = coupon.Type == CouponType.Percent ? $"{coupon.Value:0.##}% off" : $"₼{coupon.Value:0.00} off";
            text += coupon.MinOrder > 0 ? $" an order from ₼{coupon.MinOrder:0.00}" : " your order";
            if (coupon.EndsAt is not null)
            {
                text += $", until {coupon.EndsAt.Value:dd.MM.yyyy}";
            }

            return text;
        }

        public async Task<CouponCheckDto> CheckAsync(string? code, decimal total, string userId)
        {
            var value = code?.Trim().ToUpperInvariant();
            var coupon = string.IsNullOrEmpty(value)
                ? null
                : (await _couponRepo.GetAllAsync()).FirstOrDefault(m => m.Code == value);

            // Başqa statusda olan kod tapılmamış kimi göstərilir
            if (coupon is null || StatusOf(coupon) != "Active")
            {
                throw new BadRequestException("This promo code is not valid.");
            }

            // İlk sifariş kuponu yalnız hələ sifarişi olmayan müştəriyə
            if (coupon.IsFirstOrder && await HasOrdersAsync(userId))
            {
                throw new BadRequestException("This promo code is only for the first order.");
            }

            if (total < coupon.MinOrder)
            {
                throw new BadRequestException($"This promo code works from ₼{coupon.MinOrder:0.00}.");
            }

            return new CouponCheckDto
            {
                Code = coupon.Code,
                Type = coupon.Type.ToString(),
                Value = coupon.Value,
                Discount = DiscountOf(coupon, total)
            };
        }

        public async Task<CouponDto?> GetFirstOrderAsync(string userId)
        {
            if (await HasOrdersAsync(userId))
            {
                return null;
            }

            var coupon = (await _couponRepo.GetAllAsync())
                .Where(m => m.IsFirstOrder && StatusOf(m) == "Active")
                .OrderByDescending(m => m.CreatedAt)
                .FirstOrDefault();

            return coupon is null ? null : ToDto(coupon);
        }

        // Ləğv olunan sifariş sayılmır
        private async Task<bool> HasOrdersAsync(string userId)
        {
            return (await _orderRepo.GetAllAsync()).Any(m => m.UserId == userId && m.Status != OrderStatus.Cancelled);
        }

        // Endirim sifarişin məbləğindən çox ola bilməz
        private static decimal DiscountOf(Coupon coupon, decimal total)
        {
            var discount = coupon.Type == CouponType.Percent
                ? Math.Round(total * coupon.Value / 100, 2)
                : coupon.Value;

            return Math.Min(discount, total);
        }

        // Tarixlər gün kimi saxlanılır: bitmə günü də daxildir
        private static string StatusOf(Coupon coupon)
        {
            var today = DateTime.UtcNow.Date;

            if (!coupon.IsActive) return "Disabled";
            if (coupon.EndsAt is not null && today > coupon.EndsAt.Value.Date) return "Expired";
            if (coupon.UsageLimit is not null && coupon.UsedCount >= coupon.UsageLimit) return "Used up";
            if (coupon.StartsAt is not null && today < coupon.StartsAt.Value.Date) return "Scheduled";
            return "Active";
        }

        private async Task EnsureCodeIsFreeAsync(string code, int? currentId)
        {
            var coupons = await _couponRepo.GetAllAsync();
            if (coupons.Any(m => m.Id != currentId && m.Code == code))
            {
                throw new BadRequestException("This code is already used by another coupon.");
            }
        }

        private static CouponDto ToDto(Coupon coupon)
        {
            return new CouponDto
            {
                Id = coupon.Id,
                Code = coupon.Code,
                Type = coupon.Type.ToString(),
                Value = coupon.Value,
                MinOrder = coupon.MinOrder,
                UsageLimit = coupon.UsageLimit,
                UsedCount = coupon.UsedCount,
                StartsAt = coupon.StartsAt,
                EndsAt = coupon.EndsAt,
                IsActive = coupon.IsActive,
                IsFirstOrder = coupon.IsFirstOrder,
                Status = StatusOf(coupon),
                CreatedAt = coupon.CreatedAt
            };
        }
    }
}
