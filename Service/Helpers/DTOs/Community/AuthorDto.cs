namespace Service.Helpers.DTOs.Community
{
    // Post, şərh və siyahılarda göstərilən müəllif: ad (ləqəb və ya ad), ünvan açarı, şəkil və "Səviyyə, Şəhər"
    public class AuthorDto
    {
        public string UserId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Nickname { get; set; }
        public string? AvatarPath { get; set; }
        public string Role { get; set; } = string.Empty;

        // İşçi hesabıdırsa rolu (Support, Editor, Admin, SuperAdmin), adi istifadəçi üçün boş
        public string? StaffRole { get; set; }
    }
}
