using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Configurations
{
    public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.Property(m => m.ProductName).IsRequired().HasMaxLength(150);
            builder.Property(m => m.ImagePath).IsRequired().HasMaxLength(300);
            builder.Property(m => m.Price).HasPrecision(18, 2);
            builder.Property(m => m.Quantity).IsRequired();
            builder.Property(m => m.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            builder.HasOne(m => m.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(m => m.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(m => m.Product)
                .WithMany()
                .HasForeignKey(m => m.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
