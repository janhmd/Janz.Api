using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Configurations
{
    public class SavedArticleConfiguration : IEntityTypeConfiguration<SavedArticle>
    {
        public void Configure(EntityTypeBuilder<SavedArticle> builder)
        {
            builder.Property(m => m.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            builder.HasIndex(m => new { m.UserId, m.ArticleId }).IsUnique();

            builder.HasOne(m => m.User)
                .WithMany()
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(m => m.Article)
                .WithMany()
                .HasForeignKey(m => m.ArticleId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
