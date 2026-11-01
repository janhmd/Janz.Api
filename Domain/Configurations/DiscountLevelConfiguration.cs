using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Configurations
{
    public class DiscountLevelConfiguration : IEntityTypeConfiguration<DiscountLevel>
    {
        public void Configure(EntityTypeBuilder<DiscountLevel> builder)
        {
            builder.Property(m => m.Name).IsRequired().HasMaxLength(50);
            builder.Property(m => m.MinSpent).HasPrecision(18, 2);
            builder.Property(m => m.Perks).HasMaxLength(200);
            builder.Property(m => m.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            builder.HasIndex(m => m.Name).IsUnique();
        }
    }
}
