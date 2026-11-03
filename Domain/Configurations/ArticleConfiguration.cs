using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Configurations
{
    public class ArticleConfiguration : IEntityTypeConfiguration<Article>
    {
        public void Configure(EntityTypeBuilder<Article> builder)
        {
            builder.Property(m => m.Title).IsRequired().HasMaxLength(200);
            builder.Property(m => m.Slug).IsRequired().HasMaxLength(200);
            builder.Property(m => m.Topic).IsRequired().HasMaxLength(50);
            builder.Property(m => m.Lead).IsRequired().HasMaxLength(300);
            builder.Property(m => m.Body).IsRequired();
            builder.Property(m => m.CoverPath).IsRequired().HasMaxLength(300);
            builder.Property(m => m.Tags).HasMaxLength(300);
            builder.Property(m => m.AuthorName).HasMaxLength(100);
            builder.Property(m => m.AuthorRole).HasMaxLength(100);
            builder.Property(m => m.Series).HasMaxLength(100);
            builder.Property(m => m.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            builder.HasIndex(m => m.Slug).IsUnique();
        }
    }
}
