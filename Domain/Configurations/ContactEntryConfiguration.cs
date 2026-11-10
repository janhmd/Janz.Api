using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Configurations
{
    public class ContactEntryConfiguration : IEntityTypeConfiguration<ContactEntry>
    {
        public void Configure(EntityTypeBuilder<ContactEntry> builder)
        {
            builder.Property(m => m.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(m => m.Text).IsRequired().HasMaxLength(2000);
            builder.Property(m => m.StaffUserId).HasMaxLength(450);

            builder.HasIndex(m => new { m.ContactMessageId, m.CreatedAt });

            builder.HasOne(m => m.ContactMessage)
                .WithMany()
                .HasForeignKey(m => m.ContactMessageId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
