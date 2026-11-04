using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Configurations
{
    public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
    {
        public void Configure(EntityTypeBuilder<AppUser> builder)
        {
            builder.Property(m => m.Nickname).HasMaxLength(30);
            builder.Property(m => m.AvatarPath).HasMaxLength(300);
            builder.Property(m => m.Bio).HasMaxLength(180);
            builder.Property(m => m.CookMethods).HasMaxLength(500);
            builder.Property(m => m.OrganizationName).HasMaxLength(200);
            builder.Property(m => m.OrganizationAddress).HasMaxLength(300);
            builder.Property(m => m.Voen).HasMaxLength(20);
            builder.Property(m => m.TaxNumber).HasMaxLength(30);

            builder.HasIndex(m => m.Nickname).IsUnique().HasFilter("[Nickname] IS NOT NULL");
        }
    }
}
