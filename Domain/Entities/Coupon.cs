using Domain.Common;
using Domain.Enums;

namespace Domain.Entities
{
    public class Coupon : BaseEntity
    {
        public string Code { get; set; } = string.Empty;    // böyük hərflər və rəqəmlər: WELCOME10
        public CouponType Type { get; set; }
        public decimal Value { get; set; }                   // faiz (10) və ya məbləğ (5.00)
        public decimal MinOrder { get; set; }                // 0 - limit yoxdur
        public int? UsageLimit { get; set; }                 // boş - limitsiz
        public int UsedCount { get; set; }
        public DateTime? StartsAt { get; set; }
        public DateTime? EndsAt { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsFirstOrder { get; set; }               // yalnız sifarişi olmayan müştəriyə: saytda kupon kartında göstərilir
    }
}
