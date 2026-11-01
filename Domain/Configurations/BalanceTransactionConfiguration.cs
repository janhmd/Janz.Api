using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Configurations
{
    public class BalanceTransactionConfiguration : IEntityTypeConfiguration<BalanceTransaction>
    {
        public void Configure(EntityTypeBuilder<BalanceTransaction> builder)
        {
            builder.Property(m => m.Amount).HasPrecision(18, 2);
            builder.Property(m => m.Type).IsRequired();
            builder.Property(m => m.Note).HasMaxLength(200);
            builder.Property(m => m.PaymentSessionId).HasMaxLength(200);
            builder.Property(m => m.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            builder.HasIndex(m => m.PaymentSessionId);

            builder.HasOne(m => m.User)
                .WithMany()
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
