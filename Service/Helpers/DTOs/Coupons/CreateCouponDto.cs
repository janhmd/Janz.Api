namespace Service.Helpers.DTOs.Coupons
{
    // Admin: yeni kupon. Type: Percent və ya Fixed
    public class CreateCouponDto
    {
        public string? Code { get; set; }
        public string? Type { get; set; }
        public decimal Value { get; set; }
        public decimal MinOrder { get; set; }
        public int? UsageLimit { get; set; }
        public DateTime? StartsAt { get; set; }
        public DateTime? EndsAt { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
