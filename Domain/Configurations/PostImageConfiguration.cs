using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Configurations
{
    public class PostImageConfiguration : IEntityTypeConfiguration<PostImage>
    {
        public void Configure(EntityTypeBuilder<PostImage> builder)
        {
            builder.Property(m => m.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(m => m.ImagePath).IsRequired().HasMaxLength(300);
            builder.Property(m => m.Order).IsRequired();

            builder.HasOne(m => m.Post)
                .WithMany(p => p.Images)
                .HasForeignKey(m => m.PostId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
