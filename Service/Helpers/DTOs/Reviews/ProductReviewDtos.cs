namespace Service.Helpers.DTOs.Reviews
{
    public class ProductReviewDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string AuthorName { get; set; } = string.Empty;    // "Aysel M." (soyadın ilk hərfi)
        public int Rating { get; set; }
        public string? Text { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsMine { get; set; }
    }

    // Məhsul səhifəsindəki "Comments" bölməsi: orta reytinq, rəylər və istifadəçinin öz rəyi
    public class ProductReviewsDto
    {
        public decimal? Average { get; set; }
        public int Count { get; set; }
        public List<ProductReviewDto> Items { get; set; } = new List<ProductReviewDto>();
        public bool CanReview { get; set; }                        // məhsulu alıb və sifariş çatdırılıb
        public ProductReviewDto? MyReview { get; set; }
    }

    public class SaveProductReviewDto
    {
        public int Rating { get; set; }
        public string? Text { get; set; }
    }
}
