using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Configurations
{
    public class PostConfiguration : IEntityTypeConfiguration<Post>
    {
        public void Configure(EntityTypeBuilder<Post> builder)
        {
            builder.Property(m => m.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(m => m.Text).IsRequired().HasMaxLength(2000);
            builder.Property(m => m.Brew).HasMaxLength(80);
            builder.Property(m => m.Recipe).HasMaxLength(300);
            builder.Property(m => m.Dose).HasMaxLength(30);
            builder.Property(m => m.Water).HasMaxLength(30);
            builder.Property(m => m.Grind).HasMaxLength(30);
            builder.Property(m => m.Time).HasMaxLength(30);
            builder.Property(m => m.Tags).HasMaxLength(300);

            builder.HasIndex(m => m.UserId);

            builder.HasOne(m => m.User)
                .WithMany()
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Məhsul silinəndə post qalır, yalnız məhsul bağlantısı götürülür
            builder.HasOne(m => m.Product)
                .WithMany()
                .HasForeignKey(m => m.ProductId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
