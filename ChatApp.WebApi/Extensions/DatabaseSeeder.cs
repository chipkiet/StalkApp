using System;
using System.Linq;
using ChatApp.Domain.Entities;
using ChatApp.Domain.Enums;
using ChatApp.Infrastructure.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ChatApp.WebApi.Extensions;

public static class DatabaseSeeder
{
    // Fixed IDs so demo login stays stable across restarts
    public static readonly Guid ThienId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid AeChillId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid TestRoomId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    public static void SeedDatabase(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        // Bỏ dòng EnsureDeleted để không bị mất dữ liệu cũ sau mỗi lần khởi động lại Server
        // context.Database.EnsureDeleted();
        
        // Tự động apply pending migrations
        context.Database.EnsureCreated();

        if (!context.Users.Any())
        {
            logger.LogInformation("Bắt đầu mồi dữ liệu (Seeding) ảo để test...");

            var thien = new User
            {
                Id = ThienId,
                Username = "thien",
                DisplayName = "Nguyễn Lương Hoàng Thiên",
                IsOnline = true,
                LastSeenAt = DateTime.UtcNow
            };

            var aechill = new User
            {
                Id = AeChillId,
                Username = "aechill",
                DisplayName = "AE Chill",
                IsOnline = true,
                LastSeenAt = DateTime.UtcNow
            };

            context.Users.AddRange(thien, aechill);

            var conversation = new Conversation
            {
                Id = TestRoomId,
                Title = "Test Room",
                Type = ConversationType.Group,
                CreatedAt = DateTime.UtcNow
            };
            context.Conversations.Add(conversation);

            context.Participants.AddRange(
                new Participant { ConversationId = conversation.Id, UserId = thien.Id, Role = ParticipantRole.Admin, JoinedAt = DateTime.UtcNow },
                new Participant { ConversationId = conversation.Id, UserId = aechill.Id, Role = ParticipantRole.Member, JoinedAt = DateTime.UtcNow }
            );

            context.SaveChanges();

            logger.LogInformation("Seed dữ liệu thành công!");
            logger.LogWarning("Demo users: Thiên={ThienId}, AE Chill={AeChillId}, Room={RoomId}", ThienId, AeChillId, TestRoomId);
        }
    }
}
