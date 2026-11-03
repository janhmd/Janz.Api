using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Configurations
{
    public class ArticleCommentVoteConfiguration : IEntityTypeConfiguration<ArticleCommentVote>
    {
        public void Configure(EntityTypeBuilder<ArticleCommentVote> builder)
        {
            builder.Property(m => m.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            builder.HasIndex(m => new { m.UserId, m.CommentId }).IsUnique();

            // SQL Server eyni cədvələ iki kaskad yolu qəbul etmir (istifadəçi -> şərh -> səs və istifadəçi -> səs):
            // səslər şərhlə birlikdə silinir, istifadəçi tərəfində NoAction qalır (istifadəçi silinmir, yalnız bloklanır)
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
