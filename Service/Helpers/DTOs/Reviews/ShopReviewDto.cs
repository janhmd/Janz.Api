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
}
