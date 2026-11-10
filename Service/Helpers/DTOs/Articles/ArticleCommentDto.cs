namespace Service.Helpers.DTOs.Articles
{
    public class ArticleCommentDto
    {
        public int Id { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public string? AuthorAvatar { get; set; }
        public string? StaffRole { get; set; }
        public string Text { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int Likes { get; set; }
        public int Dislikes { get; set; }

        // Daxil olmuş istifadəçi üçün: öz şərhidir (silə bilər) və verdiyi səs (1, -1, 0)
        public bool IsMine { get; set; }
        public int MyVote { get; set; }
    }
}
