namespace Service.Helpers.DTOs.Reviews
{
    public class SaveShopReviewDto
    {
        public int ShopRating { get; set; }
        public string? ShopText { get; set; }
        public int DeliveryRating { get; set; }
        public string? DeliveryText { get; set; }
        public string? Comment { get; set; }
    }
}
