namespace Service.Helpers.DTOs.Dashboard
{
    // Admin panelin bildiriş mərkəzindəki bir sətir. Ayrıca cədvəldə saxlanmır, hər dəfə cari məlumatdan hesablanır
    public class AdminNotificationDto
    {
        public string Type { get; set; } = string.Empty;       // order | report | review | stock
        public string Severity { get; set; } = "info";         // info | warning | danger
        public string Title { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public string Link { get; set; } = string.Empty;       // admin içində açılacaq səhifə
        public DateTime? CreatedAt { get; set; }
    }
}
