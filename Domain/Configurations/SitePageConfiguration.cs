using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Configurations
{
    public class SitePageConfiguration : IEntityTypeConfiguration<SitePage>
    {
        public void Configure(EntityTypeBuilder<SitePage> builder)
        {
            builder.Property(m => m.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(m => m.Slug).IsRequired().HasMaxLength(80);
            builder.Property(m => m.Title).IsRequired().HasMaxLength(80);
            builder.Property(m => m.Body).IsRequired();

            builder.HasIndex(m => m.Slug).IsUnique();
        }
    }
}
