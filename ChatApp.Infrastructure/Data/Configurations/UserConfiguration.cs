using ChatApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatApp.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Username).HasMaxLength(50).IsRequired();
        builder.HasIndex(x => x.Username).IsUnique();

        builder.Property(x => x.DisplayName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.AvatarUrl).HasMaxLength(500);

        builder.Property(x => x.Email).HasMaxLength(255).IsRequired();
        builder.HasIndex(x => x.Email).IsUnique();

        builder.Property(x => x.PasswordHash).IsRequired();

        builder.Property(x => x.PhoneNumber).HasMaxLength(20);
        builder.HasIndex(x => x.PhoneNumber).IsUnique();

        builder.Property(x => x.Bio).HasMaxLength(500);
    }
}
