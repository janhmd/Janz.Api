using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Configurations
{
    public class CouponConfiguration : IEntityTypeConfiguration<Coupon>
    {
        public void Configure(EntityTypeBuilder<Coupon> builder)
        {
            builder.Property(m => m.Code).IsRequired().HasMaxLength(20);
            builder.Property(m => m.Type).IsRequired();
            builder.Property(m => m.Value).HasPrecision(18, 2);
            builder.Property(m => m.MinOrder).HasPrecision(18, 2);
            builder.Property(m => m.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            builder.HasIndex(m => m.Code).IsUnique();
        }
    }
}
