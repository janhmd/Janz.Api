namespace Service.Helpers.DTOs.Users
{
    // Admin paneldə istifadəçilər siyahısının bir sətri
    public class UserAdminDto
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsBlocked { get; set; }
        public DateTime CreatedAt { get; set; }
        public int OrderCount { get; set; }
        public decimal TotalSpent { get; set; }     // ləğv olunmuş sifarişlər sayılmır
    }
}
