using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Configurations
{
    public class SiteSettingConfiguration : IEntityTypeConfiguration<SiteSetting>
    {
        public void Configure(EntityTypeBuilder<SiteSetting> builder)
        {
            builder.Property(m => m.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(m => m.Phone).IsRequired().HasMaxLength(30);
            builder.Property(m => m.Email).IsRequired().HasMaxLength(100);
            builder.Property(m => m.Address).IsRequired().HasMaxLength(150);
            builder.Property(m => m.Instagram).IsRequired().HasMaxLength(200);
            builder.Property(m => m.WhatsApp).IsRequired().HasMaxLength(200);
            builder.Property(m => m.Hours).IsRequired().HasMaxLength(100);
        }
    }
}
