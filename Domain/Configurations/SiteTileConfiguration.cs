using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Configurations
{
    public class SiteTileConfiguration : IEntityTypeConfiguration<SiteTile>
    {
        public void Configure(EntityTypeBuilder<SiteTile> builder)
        {
            builder.Property(m => m.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(m => m.Placement).IsRequired().HasMaxLength(20);
            builder.Property(m => m.Title).IsRequired().HasMaxLength(40);
            builder.Property(m => m.Link).IsRequired().HasMaxLength(150);
            builder.Property(m => m.ImagePath).IsRequired().HasMaxLength(300);
            builder.Property(m => m.VideoPath).HasMaxLength(300);

            builder.HasIndex(m => new { m.Placement, m.SortOrder });
        }
    }
}
