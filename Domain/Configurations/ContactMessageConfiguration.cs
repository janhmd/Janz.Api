using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Configurations
{
    public class ContactMessageConfiguration : IEntityTypeConfiguration<ContactMessage>
    {
        public void Configure(EntityTypeBuilder<ContactMessage> builder)
        {
            builder.Property(m => m.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(m => m.UserId).HasMaxLength(450);
            builder.Property(m => m.Name).IsRequired().HasMaxLength(80);
            builder.Property(m => m.Email).IsRequired().HasMaxLength(150);
            builder.Property(m => m.Topic).IsRequired().HasMaxLength(20);
            builder.Property(m => m.Message).IsRequired().HasMaxLength(2000);
            builder.Property(m => m.Reply).HasMaxLength(2000);

            builder.HasIndex(m => new { m.IsAnswered, m.CreatedAt });
        }
    }
}
