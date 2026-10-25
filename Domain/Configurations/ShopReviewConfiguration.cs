using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Configurations
{
    public class ShopReviewConfiguration : IEntityTypeConfiguration<ShopReview>
    {
        public void Configure(EntityTypeBuilder<ShopReview> builder)
        {
            builder.Property(m => m.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(m => m.ShopText).HasMaxLength(500);
            builder.Property(m => m.DeliveryText).HasMaxLength(500);
            builder.Property(m => m.Comment).HasMaxLength(1000);

            builder.HasIndex(m => m.OrderId).IsUnique();

            builder.HasOne(m => m.Order)
                .WithMany()
                .HasForeignKey(m => m.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Sifariş artıq istifadəçidən asılıdır: ikinci cascade yolu olmasın deyə NoAction
            builder.HasOne(m => m.User)
                .WithMany()
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
