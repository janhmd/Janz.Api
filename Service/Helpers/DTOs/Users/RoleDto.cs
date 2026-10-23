namespace Service.Helpers.DTOs.Users
{
    // Rollar səhifəsi: rolun adı, nə iş gördüyü və neçə istifadəçidə olduğu
    public class RoleDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int UserCount { get; set; }
    }
}
