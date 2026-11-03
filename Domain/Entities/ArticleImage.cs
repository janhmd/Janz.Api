using Domain.Common;

namespace Domain.Entities
{
    public class ArticleImage : BaseEntity
    {
        public string ImagePath { get; set; } = string.Empty;
        public int Order { get; set; }
        public int ArticleId { get; set; }
        public Article Article { get; set; } = null!;
    }
}
