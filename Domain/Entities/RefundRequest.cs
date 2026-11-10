using Domain.Common;
using Domain.Enums;

namespace Domain.Entities
{
    // Müştərinin çatdırılmış sifariş üçün pulu geri istəməsi. Admin və ya Support qəbul edəndə məbləğ
    // müştərinin daxili hesabına (balansa) qayıdır, rədd edə də bilər. Bir sifarişə bir aktiv sorğu olur
    public class RefundRequest : BaseEntity
    {
        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;

        public string UserId { get; set; } = string.Empty;

        public string Reason { get; set; } = string.Empty;

        // Qəbul olunanda balansa yazılan məbləğ (sorğu anında sifarişin cəmi, işçi azalda bilər)
        public decimal Amount { get; set; }

        public RefundStatus Status { get; set; }

        // Admin/Support-un qeydi (rədd səbəbi və ya əlavə izah)
        public string? StaffNote { get; set; }
        public DateTime? ResolvedAt { get; set; }
    }
}
