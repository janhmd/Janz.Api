using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Configurations
{
    public class PostCommentConfiguration : IEntityTypeConfiguration<PostComment>
    {
        public void Configure(EntityTypeBuilder<PostComment> builder)
        {
            builder.Property(m => m.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(m => m.Text).IsRequired().HasMaxLength(1000);
            builder.Property(m => m.ImagePath).HasMaxLength(300);

            builder.HasIndex(m => m.PostId);

            builder.HasOne(m => m.User)
                .WithMany()
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(m => m.Post)
                .WithMany(p => p.Comments)
                .HasForeignKey(m => m.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            // Cavabın üstündəki şərh silinəndə cavablar xidmət tərəfində silinir (özünə kaskad olmur)
            builder.HasOne(m => m.Parent)
                .WithMany()
                .HasForeignKey(m => m.ParentId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
