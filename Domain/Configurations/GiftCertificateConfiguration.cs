using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Configurations
{
    public class GiftCertificateConfiguration : IEntityTypeConfiguration<GiftCertificate>
    {
        public void Configure(EntityTypeBuilder<GiftCertificate> builder)
        {
            builder.Property(m => m.Code).IsRequired().HasMaxLength(20);
            builder.Property(m => m.Amount).HasPrecision(18, 2);
            builder.Property(m => m.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            builder.HasIndex(m => m.Code).IsUnique();

            // İstifadəçi silinəndə sertifikat qalır, sadəcə kimin istifadə etdiyi boşalır
            builder.HasOne(m => m.UsedBy)
                .WithMany()
                .HasForeignKey(m => m.UsedById)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
