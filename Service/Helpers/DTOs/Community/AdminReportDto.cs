namespace Service.Helpers.DTOs.Community
{
    public class AdminReportDto
    {
        public int Id { get; set; }
        public string Reporter { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string Target { get; set; } = string.Empty;
        public int? PostId { get; set; }
        public int? CommentId { get; set; }
        public string TargetText { get; set; } = string.Empty;
        public string TargetAuthor { get; set; } = string.Empty;
        public bool IsResolved { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
