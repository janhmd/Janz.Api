namespace Service.Helpers.DTOs.Reviews
{
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
}
