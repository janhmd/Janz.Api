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
}
