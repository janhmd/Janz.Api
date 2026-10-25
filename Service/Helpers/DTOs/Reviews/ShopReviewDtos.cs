namespace Service.Helpers.DTOs.Reviews
{
    public class ShopReviewDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public string AuthorLevel { get; set; } = string.Empty;     // Newcomer / Regular / Coffee lover
        public int ShopRating { get; set; }
        public string? ShopText { get; set; }
        public int DeliveryRating { get; set; }
        public string? DeliveryText { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // Reviews səhifəsi və ana səhifə: ortalar, ulduzlar üzrə paylanma və rəylər
    public class ShopReviewsDto
    {
        public int Total { get; set; }
        public decimal? ShopAverage { get; set; }
        public decimal? DeliveryAverage { get; set; }
        public List<int> ShopCounts { get; set; } = new List<int>();         // [5 ulduz, 4, 3, 2, 1]
        public List<int> DeliveryCounts { get; set; } = new List<int>();
        public List<ShopReviewDto> Items { get; set; } = new List<ShopReviewDto>();
    }

    // Sifariş səhifəsi: rəy yazmaq olarmı və yazılıbsa özü
    public class OrderReviewDto
    {
        public bool CanReview { get; set; }
        public ShopReviewDto? Review { get; set; }
    }

    public class SaveShopReviewDto
    {
        public int ShopRating { get; set; }
        public string? ShopText { get; set; }
        public int DeliveryRating { get; set; }
        public string? DeliveryText { get; set; }
        public string? Comment { get; set; }
    }
}
