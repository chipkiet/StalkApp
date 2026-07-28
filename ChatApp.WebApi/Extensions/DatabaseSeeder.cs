using System;
using System.Linq;
using ChatApp.Domain.Entities;
using ChatApp.Domain.Enums;
using ChatApp.Shared.Enums;
using ChatApp.Infrastructure.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.WebApi.Extensions;

public static class DatabaseSeeder
{
    // Cố định ID cho dễ test
    public static readonly Guid AliceId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid BobId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid CharlieId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    public static readonly Guid DavidId = Guid.Parse("44444444-4444-4444-4444-444444444444");
    
    public static readonly Guid DevTeamRoomId = Guid.Parse("55555555-5555-5555-5555-555555555555");
    public static readonly Guid StickyNoteId = Guid.Parse("66666666-6666-6666-6666-666666666666");
    public static readonly Guid TaskCardId = Guid.Parse("77777777-7777-7777-7777-777777777777");

    public static void SeedDatabase(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        // Áp dụng Migrations chuẩn, KHÔNG dùng EnsureCreated
        context.Database.Migrate();

        if (!context.Users.Any())
        {
            logger.LogInformation("Bắt đầu mồi dữ liệu (Seeding) demo chuyên nghiệp...");

            // 1. Seed Users
            var alice = new User { Id = AliceId, Username = "alice", DisplayName = "Alice Admin", PhoneNumber = "0999111111", IsOnline = true, LastSeenAt = DateTime.UtcNow, KarmaPoints = 1000, GamificationTitle = "Trùm hệ thống" };
            var bob = new User { Id = BobId, Username = "bob", DisplayName = "Bob Builder", PhoneNumber = "0999222222", IsOnline = true, LastSeenAt = DateTime.UtcNow, KarmaPoints = 500, GamificationTitle = "Chiến thần Code" };
            var charlie = new User { Id = CharlieId, Username = "charlie", DisplayName = "Charlie Chaplin", PhoneNumber = "0999333333", IsOnline = false, LastSeenAt = DateTime.UtcNow.AddHours(-1), KarmaPoints = 0 };
            var david = new User { Id = DavidId, Username = "david", DisplayName = "David Developer", PhoneNumber = "0999444444", IsOnline = true, LastSeenAt = DateTime.UtcNow, KarmaPoints = 0 };
            
            context.Users.AddRange(alice, bob, charlie, david);

            // 2. Seed Friendships
            context.Friendships.AddRange(
                new Friendship { RequesterId = AliceId, AddresseeId = BobId, Status = FriendshipStatus.Accepted, CreatedAt = DateTime.UtcNow },
                new Friendship { RequesterId = CharlieId, AddresseeId = AliceId, Status = FriendshipStatus.Pending, CreatedAt = DateTime.UtcNow }
            );

            // 3. Seed Conversations & Participants
            var devTeamRoom = new Conversation { Id = DevTeamRoomId, Title = "StalkChat Dev Team", Type = ConversationType.Group, CreatedAt = DateTime.UtcNow };
            context.Conversations.Add(devTeamRoom);

            context.Participants.AddRange(
                new Participant { ConversationId = DevTeamRoomId, UserId = AliceId, Role = ParticipantRole.Admin, JoinedAt = DateTime.UtcNow },
                new Participant { ConversationId = DevTeamRoomId, UserId = BobId, Role = ParticipantRole.Member, JoinedAt = DateTime.UtcNow },
                new Participant { ConversationId = DevTeamRoomId, UserId = CharlieId, Role = ParticipantRole.Member, JoinedAt = DateTime.UtcNow },
                new Participant { ConversationId = DevTeamRoomId, UserId = DavidId, Role = ParticipantRole.Member, JoinedAt = DateTime.UtcNow }
            );

            // 4. Seed Pinboard Items
            context.PinboardItems.AddRange(
                new PinboardItem 
                { 
                    Id = StickyNoteId, ConversationId = DevTeamRoomId, Type = PinboardItemType.StickyNote, 
                    Content = "Chào mừng đến với dự án StalkChat!", PositionX = 100, PositionY = 100, ZIndex = 1, CreatedAt = DateTime.UtcNow 
                },
                new PinboardItem 
                { 
                    Id = TaskCardId, ConversationId = DevTeamRoomId, Type = PinboardItemType.Task, 
                    Content = "Thiết lập Database mới", PositionX = 400, PositionY = 150, ZIndex = 2, 
                    AssignedToUserId = BobId, IsCompleted = false, CreatedAt = DateTime.UtcNow 
                }
            );

            context.SaveChanges();

            logger.LogInformation("Seed dữ liệu thành công!");
            logger.LogWarning("Demo accounts: Alice={Alice}, Bob={Bob}, Charlie={Charlie}, David={David}", AliceId, BobId, CharlieId, DavidId);
        }
    }
}
