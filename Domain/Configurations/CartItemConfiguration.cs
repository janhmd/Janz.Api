using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Configurations
{
    public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
    {
        public void Configure(EntityTypeBuilder<CartItem> builder)
        {
            builder.Property(m => m.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(m => m.Quantity).IsRequired();

            builder.Property(m => m.Grind).HasMaxLength(40);

            // Eyni məhsul fərqli variant və ya üyütmə ilə ayrıca sətir olur: birləşdirməni servis edir
            builder.HasIndex(m => new { m.UserId, m.ProductId });

            builder.HasOne(m => m.User)
                .WithMany()
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(m => m.Product)
                .WithMany()
                .HasForeignKey(m => m.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // SQL Server bir cədvələ iki cascade yolu icazə vermir: variant silinəndə sətirləri servis silir
            builder.HasOne(m => m.Variant)
                .WithMany()
                .HasForeignKey(m => m.VariantId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
