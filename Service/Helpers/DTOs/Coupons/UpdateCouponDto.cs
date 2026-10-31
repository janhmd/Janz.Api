namespace Service.Helpers.DTOs.Coupons
{
    // Admin: dəyişdirilən kupon. Type: Percent və ya Fixed
    public class UpdateCouponDto
    {
        public string? Code { get; set; }
        public string? Type { get; set; }
        public decimal Value { get; set; }
        public decimal MinOrder { get; set; }
        public int? UsageLimit { get; set; }
        public DateTime? StartsAt { get; set; }
        public DateTime? EndsAt { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsFirstOrder { get; set; }
    }
}
