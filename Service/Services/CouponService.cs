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
        private readonly IValidator<CreateCouponDto> _createValidator;
        private readonly IValidator<UpdateCouponDto> _updateValidator;

        public CouponService(IBaseRepository<Coupon> couponRepo,
                             IValidator<CreateCouponDto> createValidator,
                             IValidator<UpdateCouponDto> updateValidator)
        {
            _couponRepo = couponRepo;
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
                IsActive = model.IsActive
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

        public async Task<CouponCheckDto> CheckAsync(string? code, decimal total)
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
                Status = StatusOf(coupon),
                CreatedAt = coupon.CreatedAt
            };
        }
    }
}
