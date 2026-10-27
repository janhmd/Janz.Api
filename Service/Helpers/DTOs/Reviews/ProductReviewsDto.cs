namespace Service.Helpers.DTOs.Reviews
{
    // Məhsul səhifəsindəki "Comments" bölməsi: orta reytinq, rəylər və istifadəçinin öz rəyi
    public class ProductReviewsDto
    {
        public decimal? Average { get; set; }
        public int Count { get; set; }
        public List<ProductReviewDto> Items { get; set; } = new List<ProductReviewDto>();
        public bool CanReview { get; set; }                        // məhsulu alıb və sifariş çatdırılıb
        public ProductReviewDto? MyReview { get; set; }
    }
}
