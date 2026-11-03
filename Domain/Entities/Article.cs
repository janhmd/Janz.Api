using Domain.Common;

namespace Domain.Entities
{
    // Jurnal məqaləsi. Mətn sadə qaydalarla yazılır: boş sətir yeni abzasdır, "## " ilə başlayan sətir başlıqdır,
    // "[photo 2: caption]" sətri məqalənin 2-ci şəklini qoyur (ArticleImage, Order = 2)
    public class Article : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Topic { get; set; } = string.Empty;
        public string Lead { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string CoverPath { get; set; } = string.Empty;

        // Vergüllə ayrılmış teqlər: "Brewing,Espresso"
        public string? Tags { get; set; }

        public string? AuthorName { get; set; }
        public string? AuthorRole { get; set; }

        // "Learning material" nişanı və seriya adı (məs. "Brewing espresso")
        public bool IsLearning { get; set; }
        public string? Series { get; set; }

        public bool IsPublished { get; set; }
        public DateTime? PublishedAt { get; set; }
        public int Views { get; set; }

        public ICollection<ArticleImage> Images { get; set; } = new List<ArticleImage>();
    }
}
