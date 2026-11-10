namespace Service.Helpers.DTOs.Coupons
{
    // Admin: kupon kimlərə göndərilir. Audience: All (bütün müştərilər) və ya Selected (UserIds-dəkilər)
    public class NotifyCouponDto
    {
        public string? Audience { get; set; }
        public List<string>? UserIds { get; set; }
    }
}
