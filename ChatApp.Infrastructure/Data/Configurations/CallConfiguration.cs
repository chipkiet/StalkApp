using ChatApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatApp.Infrastructure.Data.Configurations;

public class CallConfiguration : IEntityTypeConfiguration<Call>
{
    public void Configure(EntityTypeBuilder<Call> builder)
    {
        builder.HasKey(x => x.Id);

        // Cuộc gọi diễn ra ở Group/Private chat nào
        builder.HasOne(x => x.Conversation)
            .WithMany(x => x.Calls)
            .HasForeignKey(x => x.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ai là người gọi
        builder.HasOne(x => x.Caller)
            .WithMany(x => x.CallsInitiated)
            .HasForeignKey(x => x.CallerId)
            .OnDelete(DeleteBehavior.Restrict); // Restrict để tránh vòng lặp xóa với User

        builder.HasIndex(x => x.ConversationId);
        builder.HasIndex(x => x.CallerId);
    }
}
