namespace Service.Helpers.DTOs.Community
{
    public class UpdateCommunityProfileDto
    {
        public string? Bio { get; set; }
        public List<string> CookMethods { get; set; } = new List<string>();
    }
}
