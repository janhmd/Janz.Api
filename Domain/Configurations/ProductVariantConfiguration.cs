using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Configurations
{
    public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
    {
        public void Configure(EntityTypeBuilder<ProductVariant> builder)
        {
            builder.Property(m => m.Format).IsRequired().HasMaxLength(60);
            builder.Property(m => m.Size).HasMaxLength(30);
            builder.Property(m => m.Price).HasPrecision(18, 2);
            builder.Property(m => m.OldPrice).HasPrecision(18, 2);
            builder.Property(m => m.Stock).IsRequired();
            builder.Property(m => m.Order).IsRequired();
            builder.Property(m => m.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            builder.HasOne(m => m.Product)
                .WithMany(p => p.Variants)
                .HasForeignKey(m => m.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
