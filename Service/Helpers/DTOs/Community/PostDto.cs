namespace Service.Helpers.DTOs.Community
{
    public class PostDto
    {
        public int Id { get; set; }
        public AuthorDto Author { get; set; } = new AuthorDto();
        public string Text { get; set; } = string.Empty;
        public List<string> Images { get; set; } = new List<string>();
        public PostProductDto? Product { get; set; }
        public int? Rating { get; set; }
        public string? Brew { get; set; }
        public string? Recipe { get; set; }
        public string? Dose { get; set; }
        public string? Water { get; set; }
        public string? Grind { get; set; }
        public string? Time { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
        public DateTime CreatedAt { get; set; }
        public int Score { get; set; }
        public int MyVote { get; set; }
        public bool IsSaved { get; set; }
        public bool IsMine { get; set; }
        public bool IsFollowing { get; set; }
        public int CommentCount { get; set; }
        public List<PostCommentDto> Comments { get; set; } = new List<PostCommentDto>();
    }
}
