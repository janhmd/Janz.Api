using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Configurations
{
    public class PostReportConfiguration : IEntityTypeConfiguration<PostReport>
    {
        public void Configure(EntityTypeBuilder<PostReport> builder)
        {
            builder.Property(m => m.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(m => m.Reason).IsRequired().HasMaxLength(200);

            builder.HasIndex(m => m.IsResolved);

            builder.HasOne(m => m.User)
                .WithMany()
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(m => m.Post)
                .WithMany()
                .HasForeignKey(m => m.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            // Şərh silinəndə şikayətləri xidmət tərəfində silinir (post və şərh iki kaskad yolu verərdi)
            builder.HasOne(m => m.Comment)
                .WithMany()
                .HasForeignKey(m => m.CommentId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
