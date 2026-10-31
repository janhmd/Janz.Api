using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Configurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.Property(m => m.Status).IsRequired();
            builder.Property(m => m.Total).HasPrecision(18, 2);
            builder.Property(m => m.CouponCode).HasMaxLength(20);
            builder.Property(m => m.Discount).HasPrecision(18, 2);
            builder.Property(m => m.PaymentSessionId).HasMaxLength(200);

            // Uzunluqlar AddressConfiguration ilə eynidir (ünvan olduğu kimi köçürülür)
            builder.Property(m => m.DeliveryFullName).IsRequired().HasMaxLength(80);
            builder.Property(m => m.DeliveryPhone).IsRequired().HasMaxLength(30);
            builder.Property(m => m.DeliveryLine).IsRequired().HasMaxLength(200);
            builder.Property(m => m.DeliveryCity).IsRequired().HasMaxLength(80);
            builder.Property(m => m.DeliveryPostalCode).IsRequired().HasMaxLength(20);
            builder.Property(m => m.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            builder.HasIndex(m => m.UserId);

            builder.HasOne(m => m.User)
                .WithMany()
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
