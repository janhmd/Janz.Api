using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Configurations
{
    public class AddressConfiguration : IEntityTypeConfiguration<Address>
    {
        public void Configure(EntityTypeBuilder<Address> builder)
        {
            builder.Property(m => m.FullName).IsRequired().HasMaxLength(80);
            builder.Property(m => m.Phone).IsRequired().HasMaxLength(30);
            builder.Property(m => m.Line).IsRequired().HasMaxLength(200);
            builder.Property(m => m.City).IsRequired().HasMaxLength(80);
            builder.Property(m => m.PostalCode).IsRequired().HasMaxLength(20);
            builder.Property(m => m.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            builder.HasIndex(m => m.UserId);

            builder.HasOne(m => m.User)
                .WithMany()
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
