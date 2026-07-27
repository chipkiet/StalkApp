using ChatApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatApp.Infrastructure.Data.Configurations;

public class PinboardConnectionConfiguration : IEntityTypeConfiguration<PinboardConnection>
{
    public void Configure(EntityTypeBuilder<PinboardConnection> builder)
    {
        builder.ToTable("PinboardConnections");
        
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Label)
            .HasMaxLength(100);

        builder.HasOne(e => e.SourceItem)
            .WithMany()
            .HasForeignKey(e => e.SourceItemId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(e => e.TargetItem)
            .WithMany()
            .HasForeignKey(e => e.TargetItemId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasIndex(e => e.ConversationId);
    }
}
