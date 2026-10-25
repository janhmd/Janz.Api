using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.Property(m => m.Name).IsRequired().HasMaxLength(150);
            builder.Property(m => m.Description).IsRequired().HasMaxLength(500);
            builder.Property(m => m.ImagePath).IsRequired().HasMaxLength(300);
            builder.Property(m => m.Price).HasPrecision(18, 2);
            builder.Property(m => m.Stock).IsRequired();
            builder.Property(m => m.OldPrice).HasPrecision(18, 2);
            builder.Property(m => m.Badge).HasMaxLength(30);
            builder.Property(m => m.Roast).HasMaxLength(50);
            builder.Property(m => m.Region).HasMaxLength(50);
            builder.Property(m => m.Processing).HasMaxLength(50);
            builder.Property(m => m.Caffeine).HasMaxLength(20);
            builder.Property(m => m.Brewing).HasMaxLength(1000);
            builder.Property(m => m.Storage).HasMaxLength(500);
            builder.Property(m => m.BrewMethods).HasMaxLength(300);
            builder.Property(m => m.Use).HasMaxLength(50);
            builder.Property(m => m.Taste).HasMaxLength(60);
            builder.Property(m => m.Notes).HasMaxLength(200);
            builder.Property(m => m.TasteGroup).HasMaxLength(30);
            builder.Property(m => m.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            builder.HasOne(m => m.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(m => m.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
