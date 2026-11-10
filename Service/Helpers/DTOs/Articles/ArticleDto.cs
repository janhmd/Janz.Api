namespace Service.Helpers.DTOs.Articles
{
    // Məqalə səhifəsi: kartın məlumatları + mətn, şəkillər və müəllif
    public class ArticleDto : ArticleCardDto
    {
        public string Body { get; set; } = string.Empty;
        public List<string> Images { get; set; } = new List<string>();
        public string? AuthorName { get; set; }
        public string? AuthorRole { get; set; }
        public string? AuthorAvatar { get; set; }
    }
}
