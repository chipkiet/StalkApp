using ChatApp.Domain.Entities;
using ChatApp.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatApp.Infrastructure.Data.Configurations;

public class FriendshipConfiguration : IEntityTypeConfiguration<Friendship>
{
    public void Configure(EntityTypeBuilder<Friendship> builder)
    {
        builder.HasKey(x => x.Id);

        // Index unique để đảm bảo không có 2 lời mời trùng nhau giữa cùng 2 người
        builder.HasIndex(x => new { x.RequesterId, x.AddresseeId }).IsUnique();

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .HasDefaultValue(FriendshipStatus.Pending);

        // Quan hệ với Requester (người gửi)
        builder.HasOne(x => x.Requester)
            .WithMany(u => u.FriendshipsSent)
            .HasForeignKey(x => x.RequesterId)
            .OnDelete(DeleteBehavior.Cascade);

        // Quan hệ với Addressee (người nhận)
        builder.HasOne(x => x.Addressee)
            .WithMany(u => u.FriendshipsReceived)
            .HasForeignKey(x => x.AddresseeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
