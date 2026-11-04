namespace Service.Helpers.DTOs.Community
{
    public class PostCommentDto
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public int? ParentId { get; set; }
        public AuthorDto Author { get; set; } = new AuthorDto();
        public string Text { get; set; } = string.Empty;
        public string? ImagePath { get; set; }
        public DateTime CreatedAt { get; set; }
        public int Score { get; set; }
        public int MyVote { get; set; }
        public bool IsMine { get; set; }
        public List<PostCommentDto> Replies { get; set; } = new List<PostCommentDto>();
    }
}
