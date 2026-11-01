using Domain.Common;

namespace Domain.Entities
{
    // Endirim səviyyəsi: çatdırılmış sifarişlərə xərclənən məbləğə görə hər sifarişə avtomatik endirim
    public class DiscountLevel : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public decimal MinSpent { get; set; }                // bu məbləğdən başlayır
        public int Percent { get; set; }                     // 0-50
        public string? Perks { get; set; }
    }
}
