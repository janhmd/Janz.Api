namespace Service.Helpers.DTOs.Reviews
{
    // Admin panel: hər iki növ rəy bir siyahıda (Type = "product" və ya "shop")
    public class AdminReviewDto
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public int? ProductId { get; set; }
        public string? ProductName { get; set; }
        public int? OrderId { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public string AuthorEmail { get; set; } = string.Empty;
        public int Rating { get; set; }                      // məhsul ulduzu və ya mağaza ulduzu
        public int? DeliveryRating { get; set; }
        public string? Text { get; set; }                    // bütün mətnlər birlikdə
        public bool IsHidden { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
