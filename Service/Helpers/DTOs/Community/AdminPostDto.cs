namespace Service.Helpers.DTOs.Community
{
    // Admin: post (gizli olanlar da), şikayət sayı ilə
    public class AdminPostDto
    {
        public int Id { get; set; }
        public AuthorDto Author { get; set; } = new AuthorDto();
        public string Text { get; set; } = string.Empty;
        public bool IsHidden { get; set; }
        public int Reports { get; set; }
        public int Score { get; set; }
        public int CommentCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
