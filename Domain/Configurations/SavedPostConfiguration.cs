using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Configurations
{
    public class SavedPostConfiguration : IEntityTypeConfiguration<SavedPost>
    {
        public void Configure(EntityTypeBuilder<SavedPost> builder)
        {
            builder.Property(m => m.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            builder.HasIndex(m => new { m.UserId, m.PostId }).IsUnique();

            // Saxlananlar postla birlikdə silinir, istifadəçi tərəfi NoAction qalır (iki kaskad yolu olmasın)

            builder.HasOne(m => m.User)
                .WithMany()
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(m => m.Post)
                .WithMany()
                .HasForeignKey(m => m.PostId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
