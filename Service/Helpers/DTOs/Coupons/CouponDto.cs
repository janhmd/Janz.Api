namespace Service.Helpers.DTOs.Coupons
{
    public class CouponDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;     // Percent / Fixed
        public decimal Value { get; set; }
        public decimal MinOrder { get; set; }
        public int? UsageLimit { get; set; }
        public int UsedCount { get; set; }
        public DateTime? StartsAt { get; set; }
        public DateTime? EndsAt { get; set; }
        public bool IsActive { get; set; }
        public string Status { get; set; } = string.Empty;   // Active / Scheduled / Expired / Used up / Disabled
        public DateTime CreatedAt { get; set; }
    }
}
