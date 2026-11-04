namespace Service.Helpers.DTOs.Community
{
    public class AdminPostCommentDto
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public AuthorDto Author { get; set; } = new AuthorDto();
        public string Text { get; set; } = string.Empty;
        public string PostText { get; set; } = string.Empty;
        public bool IsHidden { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
