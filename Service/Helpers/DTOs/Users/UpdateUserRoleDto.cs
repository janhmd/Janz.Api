namespace Service.Helpers.DTOs.Users
{
    // SuperAdmin: istifadəçinin yeni rolu (User, Support, Editor, Admin, SuperAdmin)
    public class UpdateUserRoleDto
    {
        public string? Role { get; set; }
    }
}
