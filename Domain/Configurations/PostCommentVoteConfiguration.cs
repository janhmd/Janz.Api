using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Configurations
{
    public class PostCommentVoteConfiguration : IEntityTypeConfiguration<PostCommentVote>
    {
        public void Configure(EntityTypeBuilder<PostCommentVote> builder)
        {
            builder.Property(m => m.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            builder.HasIndex(m => new { m.UserId, m.CommentId }).IsUnique();

            builder.HasOne(m => m.User)
                .WithMany()
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(m => m.Comment)
                .WithMany(c => c.Votes)
                .HasForeignKey(m => m.CommentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
