namespace Service.Helpers.DTOs.Coupons
{
    // Səbətdə yazılan kodun nəticəsi: endirim məbləği
    public class CouponCheckDto
    {
        public string Code { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public decimal Discount { get; set; }
    }
}
