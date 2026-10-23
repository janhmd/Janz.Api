namespace Repository.Data;

// appsettings -> "SeedAdmins": işçi hesablarının siyahısı. Hər sətir bir hesabdır.
public class SeedAdminSettings
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;        // Roles.cs-dəki adlardan biri (SuperAdmin, Admin, Editor, Support)
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
}
