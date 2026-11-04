namespace Service.Helpers.DTOs.Community
{
    public class CommunityProfileDto
    {
        public AuthorDto Author { get; set; } = new AuthorDto();
        public string? Bio { get; set; }
        public List<string> CookMethods { get; set; } = new List<string>();
        public int Followers { get; set; }
        public int Following { get; set; }
        public bool IsMe { get; set; }
        public bool IsFollowing { get; set; }
        public bool IsBlocked { get; set; }
        public CommunityStatsDto Stats { get; set; } = new CommunityStatsDto();
        public List<PostDto> Posts { get; set; } = new List<PostDto>();
    }
}
