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
    public static void SeedDatabase(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        // Xóa DB cũ để cập nhật tên mới
        context.Database.EnsureDeleted();
        
        // Tự động apply pending migrations
        context.Database.EnsureCreated();

        if (!context.Users.Any())
        {
            logger.LogInformation("Bắt đầu mồi dữ liệu (Seeding) ảo để test...");

            // 1. Tạo User ảo
            var thien = new User
            {
                Id = Guid.NewGuid(),
                Username = "thien",
                Email = "thien@stalk.com",
                PasswordHash = "dummy_hash",
                DisplayName = "Nguyễn Lương Hoàng Thiên",
                IsOnline = true,
                LastSeenAt = DateTime.UtcNow
            };

            var aechill = new User
            {
                Id = Guid.NewGuid(),
                Username = "aechill",
                Email = "aechill@stalk.com",
                PasswordHash = "dummy_hash",
                DisplayName = "AE Chill",
                IsOnline = true,
                LastSeenAt = DateTime.UtcNow
            };

            context.Users.AddRange(thien, aechill);

            // 2. Tạo Phòng Chat (Conversation)
            var conversation = new Conversation
            {
                Id = Guid.NewGuid(),
                Title = "Test Room",
                Type = ConversationType.Group,
                CreatedAt = DateTime.UtcNow
            };
            context.Conversations.Add(conversation);

            // 3. Thêm Thiên và AE Chill vào Phòng
            context.Participants.AddRange(
                new Participant { ConversationId = conversation.Id, UserId = thien.Id, Role = ParticipantRole.Admin, JoinedAt = DateTime.UtcNow },
                new Participant { ConversationId = conversation.Id, UserId = aechill.Id, Role = ParticipantRole.Member, JoinedAt = DateTime.UtcNow }
            );

            context.SaveChanges();

            logger.LogInformation("Seed dữ liệu thành công!");
            logger.LogWarning("--------------------------------------------------");
            logger.LogWarning($"THIÊN ID: {thien.Id}");
            logger.LogWarning($"AE CHILL ID: {aechill.Id}");
            logger.LogWarning($"CONVERSATION ID: {conversation.Id}");
            logger.LogWarning("COPY CÁC ID NÀY ĐỂ NHẬP VÀO FILE INDEX.HTML NHÉ!");
            logger.LogWarning("--------------------------------------------------");
        }
        else
        {
            // In lại ID ra console cho dễ copy
            var thien = context.Users.FirstOrDefault(u => u.DisplayName.Contains("Thiên"));
            var conv = context.Conversations.FirstOrDefault();
            if (thien != null && conv != null)
            {
                logger.LogInformation($"Nhắc lại THIÊN ID: {thien.Id}");
                logger.LogInformation($"Nhắc lại CONVERSATION ID: {conv.Id}");
            }
        }
    }
}
