using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using ChatApp.Application.Features.Messages.Commands.SendMessage;
using ChatApp.Domain.Enums;
using ChatApp.Infrastructure.Data;
using ChatApp.Application.Interfaces.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SignalR;
using ChatApp.WebApi.Hubs;

namespace ChatApp.WebApi.Services;

public class ChatBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ChatBackgroundService> _logger;

    public ChatBackgroundService(IServiceScopeFactory scopeFactory, ILogger<ChatBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ChatBackgroundService is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                    var presence = scope.ServiceProvider.GetRequiredService<IPresenceTracker>();

                    var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<ChatHub>>();

                    var now = DateTime.UtcNow;

                    // 1. Process Scheduled Messages
                    var pendingMessages = await dbContext.ScheduledMessages
                        .Where(m => !m.IsSent && m.ScheduledAt <= now)
                        .ToListAsync(stoppingToken);

                    foreach (var sm in pendingMessages)
                    {
                        try
                        {
                            var command = new SendMessageCommand(
                                sm.ConversationId,
                                sm.SenderId,
                                MessageType.System,
                                $"Hệ thống: {sm.Content}"
                            );
                            var messageDto = await mediator.Send(command, stoppingToken);

                            // Phát qua SignalR cho các client đang mở phòng chat
                            await hubContext.Clients.Group(sm.ConversationId.ToString())
                                .SendAsync("ReceiveNewMessage", messageDto);

                            sm.IsSent = true;
                            dbContext.ScheduledMessages.Update(sm);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to send scheduled message {MessageId}", sm.Id);
                        }
                    }

                    // 2. Ghost Presence Sync
                    var onlineUsers = await dbContext.Users
                        .Where(u => u.IsOnline)
                        .ToListAsync(stoppingToken);

                    var activeUserIds = (await presence.GetOnlineUsersAsync()).ToList();

                    bool hasPresenceChanges = false;
                    foreach (var user in onlineUsers)
                    {
                        if (!activeUserIds.Contains(user.Id))
                        {
                            // User is marked online in DB but not present in memory trackers
                            user.IsOnline = false;
                            user.UpdatedAt = now;
                            dbContext.Users.Update(user);
                            hasPresenceChanges = true;

                            // Thông báo toàn server rằng user này đã offline
                            await hubContext.Clients.All.SendAsync("UserOffline", new { userId = user.Id, lastSeenAt = user.UpdatedAt });
                        }
                    }

                    if (pendingMessages.Any() || hasPresenceChanges)
                    {
                        await dbContext.SaveChangesAsync(stoppingToken);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred executing ChatBackgroundService.");
            }

            // Chạy lặp mỗi 5 giây thay vì 1 phút để giảm độ trễ (delay) khi xuất tin nhắn
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }

        _logger.LogInformation("ChatBackgroundService is stopping.");
    }
}
