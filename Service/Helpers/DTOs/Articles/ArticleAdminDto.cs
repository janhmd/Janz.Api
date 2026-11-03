namespace Service.Helpers.DTOs.Articles
{
    // Admin panel: dərc olunmamış məqalələr də görünür
    public class ArticleAdminDto : ArticleDto
    {
        public bool IsPublished { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
