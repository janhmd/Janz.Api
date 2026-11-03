using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Configurations
{
    public class ArticleImageConfiguration : IEntityTypeConfiguration<ArticleImage>
    {
        public void Configure(EntityTypeBuilder<ArticleImage> builder)
        {
            builder.Property(m => m.ImagePath).IsRequired().HasMaxLength(300);
            builder.Property(m => m.Order).IsRequired();
            builder.Property(m => m.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            builder.HasOne(m => m.Article)
                .WithMany(a => a.Images)
                .HasForeignKey(m => m.ArticleId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
