namespace Service.Helpers.DTOs.Reviews
{
    // Sifariş səhifəsi: rəy yazmaq olarmı və yazılıbsa özü
    public class OrderReviewDto
    {
        public bool CanReview { get; set; }
        public ShopReviewDto? Review { get; set; }
    }
}
