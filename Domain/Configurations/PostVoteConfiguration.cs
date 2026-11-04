using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Configurations
{
    public class PostVoteConfiguration : IEntityTypeConfiguration<PostVote>
    {
        public void Configure(EntityTypeBuilder<PostVote> builder)
        {
            builder.Property(m => m.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            builder.HasIndex(m => new { m.UserId, m.PostId }).IsUnique();

            // SQL Server eyni cədvələ iki kaskad yolu qəbul etmir: səslər postla birlikdə silinir, istifadəçi tərəfi NoAction qalır

            builder.HasOne(m => m.User)
                .WithMany()
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(m => m.Post)
                .WithMany(p => p.Votes)
                .HasForeignKey(m => m.PostId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
