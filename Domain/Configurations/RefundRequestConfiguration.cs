using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Configurations
{
    public class RefundRequestConfiguration : IEntityTypeConfiguration<RefundRequest>
    {
        public void Configure(EntityTypeBuilder<RefundRequest> builder)
        {
            builder.Property(m => m.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(m => m.UserId).IsRequired().HasMaxLength(450);
            builder.Property(m => m.Reason).IsRequired().HasMaxLength(500);
            builder.Property(m => m.Amount).HasPrecision(18, 2);
            builder.Property(m => m.Status).IsRequired();
            builder.Property(m => m.StaffNote).HasMaxLength(500);

            builder.HasIndex(m => m.OrderId);
            builder.HasIndex(m => new { m.Status, m.CreatedAt });

            // Sifariş silinməz: sorğu tarixçəsi qalır
            builder.HasOne(m => m.Order)
                .WithMany()
                .HasForeignKey(m => m.OrderId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
