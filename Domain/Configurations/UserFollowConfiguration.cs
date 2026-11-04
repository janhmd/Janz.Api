using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Configurations
{
    public class UserFollowConfiguration : IEntityTypeConfiguration<UserFollow>
    {
        public void Configure(EntityTypeBuilder<UserFollow> builder)
        {
            builder.Property(m => m.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            builder.HasIndex(m => new { m.FollowerId, m.FollowedId }).IsUnique();
            builder.HasIndex(m => m.FollowedId);

            // İki istifadəçi açarı da eyni cədvələ gedir: kaskad yoxdur (istifadəçi silinmir, yalnız bloklanır)

            builder.HasOne(m => m.Follower)
                .WithMany()
                .HasForeignKey(m => m.FollowerId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(m => m.Followed)
                .WithMany()
                .HasForeignKey(m => m.FollowedId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
