namespace Service.Helpers.DTOs.Community
{
    // Subscribers / subscriptions / blacklist siyahısındakı istifadəçi
    public class CommunityPersonDto
    {
        public AuthorDto Author { get; set; } = new AuthorDto();
        public bool IsFollowing { get; set; }
        public bool IsMe { get; set; }
    }
}
