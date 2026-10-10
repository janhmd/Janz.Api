using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Configurations
{
    public class ProductTagConfiguration : IEntityTypeConfiguration<ProductTag>
    {
        public void Configure(EntityTypeBuilder<ProductTag> builder)
        {
            builder.Property(m => m.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            builder.HasIndex(m => new { m.ProductId, m.TagId }).IsUnique();

            builder.HasOne(m => m.Product)
                .WithMany(p => p.ProductTags)
                .HasForeignKey(m => m.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(m => m.Tag)
                .WithMany(t => t.ProductTags)
                .HasForeignKey(m => m.TagId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
