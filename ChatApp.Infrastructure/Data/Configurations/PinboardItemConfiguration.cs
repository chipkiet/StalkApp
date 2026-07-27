using ChatApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatApp.Infrastructure.Data.Configurations;

public class PinboardItemConfiguration : IEntityTypeConfiguration<PinboardItem>
{
    public void Configure(EntityTypeBuilder<PinboardItem> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Content)
            .IsRequired(false)
            .HasMaxLength(2000);

        builder.HasOne(x => x.Conversation)
            .WithMany(c => c.PinboardItems)
            .HasForeignKey(x => x.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.LinkedMessage)
            .WithMany(m => m.LinkedPinboardItems)
            .HasForeignKey(x => x.LinkedMessageId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.AssignedToUser)
            .WithMany(u => u.AssignedTasks)
            .HasForeignKey(x => x.AssignedToUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
