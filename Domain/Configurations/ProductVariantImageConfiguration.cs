using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Configurations
{
    public class ProductVariantImageConfiguration : IEntityTypeConfiguration<ProductVariantImage>
    {
        public void Configure(EntityTypeBuilder<ProductVariantImage> builder)
        {
            builder.Property(m => m.ImagePath).IsRequired().HasMaxLength(300);
            builder.Property(m => m.Order).IsRequired();
            builder.Property(m => m.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            builder.HasOne(m => m.Variant)
                .WithMany(v => v.Images)
                .HasForeignKey(m => m.VariantId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
