using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Configurations
{
    public class UserBlockConfiguration : IEntityTypeConfiguration<UserBlock>
    {
        public void Configure(EntityTypeBuilder<UserBlock> builder)
        {
            builder.Property(m => m.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            builder.HasIndex(m => new { m.UserId, m.BlockedId }).IsUnique();

            builder.HasOne(m => m.User)
                .WithMany()
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(m => m.Blocked)
                .WithMany()
                .HasForeignKey(m => m.BlockedId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
