using System;
using System.Security.Claims;
using System.Threading.Tasks;
using ChatApp.Application.DTOs.Messages;
using ChatApp.Application.Features.Auth.Commands.ApproveQrSession;
using ChatApp.Application.Features.Messages.Commands.AddReaction;
using ChatApp.Application.Features.Messages.Commands.DeleteMessage;
using ChatApp.Application.Features.Messages.Commands.EditMessage;
using ChatApp.Application.Features.Messages.Commands.ForwardMessage;
using ChatApp.Application.Features.Messages.Commands.PinMessage;
using ChatApp.Application.Features.Messages.Commands.RemoveReaction;
using ChatApp.Application.Features.Messages.Commands.SendMessage;
using ChatApp.Application.Interfaces.Repositories;
using ChatApp.Application.Interfaces.Services;
using ChatApp.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;

namespace ChatApp.WebApi.Hubs;

[Authorize] // Yêu cầu JWT để kết nối Hub
public class ChatHub : Hub
{
    private readonly IMediator _mediator;
    private readonly IPresenceTracker _presenceTracker;
    private readonly IGenericRepository<User> _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMemoryCache _cache;

    public ChatHub(
        IMediator mediator,
        IPresenceTracker presenceTracker,
        IGenericRepository<User> userRepository,
        IUnitOfWork unitOfWork,
        IMemoryCache cache)
    {
        _mediator = mediator;
        _presenceTracker = presenceTracker;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    // ─── Helper: Lấy UserId từ JWT Claims ───────────────────────────────────
    private Guid GetCurrentUserId()
    {
        var claim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? Context.User?.FindFirst("sub")?.Value 
            ?? Context.User?.FindFirst("nameid")?.Value;
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }

    // ─── UC-22: Presence Tracking ─────────────────────────────────────────
    public override async Task OnConnectedAsync()
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
        {
            await base.OnConnectedAsync();
            return;
        }

        // Đăng ký connection, nếu đây là kết nối đầu tiên → User vừa Online
        var justCameOnline = await _presenceTracker.UserConnectedAsync(userId, Context.ConnectionId);

        if (justCameOnline)
        {
            // Cập nhật DB
            var user = await _userRepository.GetByIdAsync(userId);
            if (user is not null)
            {
                user.IsOnline = true;
                user.UpdatedAt = DateTime.UtcNow;
                _userRepository.Update(user);
                await _unitOfWork.SaveChangesAsync();
            }

            // Thông báo cho tất cả client biết user này vừa Online
            await Clients.All.SendAsync("UserOnline", userId);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetCurrentUserId();
        if (userId != Guid.Empty)
        {
            // Nếu đây là kết nối cuối cùng → User vừa Offline
            var justWentOffline = await _presenceTracker.UserDisconnectedAsync(userId, Context.ConnectionId);

            if (justWentOffline)
            {
                var lastSeenAt = DateTime.UtcNow;

                // Cập nhật DB
                var user = await _userRepository.GetByIdAsync(userId);
                if (user is not null)
                {
                    user.IsOnline = false;
                    user.LastSeenAt = lastSeenAt;
                    user.UpdatedAt = lastSeenAt;
                    _userRepository.Update(user);
                    await _unitOfWork.SaveChangesAsync();
                }

                // Thông báo cho tất cả client biết user này vừa Offline
                await Clients.All.SendAsync("UserOffline", new { userId, lastSeenAt });
            }
        }

        await base.OnDisconnectedAsync(exception);
    }

    // ─── UC-21: QR Code Login – Web/Desktop subscribe vào group để chờ JWT ─
    public async Task SubscribeQrSession(string sessionId)
    {
        // Web/Desktop client join vào group riêng cho session này
        await Groups.AddToGroupAsync(Context.ConnectionId, $"QR_{sessionId}");

        // Kiểm tra ngay nếu QR đã được approve (tránh trường hợp subscribe muộn)
        var cacheKey = ApproveQrSessionCommandHandler.QR_SESSION_PREFIX + sessionId;
        if (_cache.TryGetValue(cacheKey, out QrSessionState? state) && state?.IsApproved == true)
        {
            // QR đã được approve → gửi ngay JWT về client này
            await Clients.Caller.SendAsync("QrLoginApproved", state.AuthResponse);
        }
    }

    // ─── UC-21: Server gọi khi QR được approve từ ApproveQrSessionCommand ──
    // (Được gọi thủ công từ AuthController sau khi approve thành công)
    // → Thực ra ta push từ Hub method riêng bên dưới
    public async Task NotifyQrApproved(string sessionId)
    {
        var cacheKey = ApproveQrSessionCommandHandler.QR_SESSION_PREFIX + sessionId;
        if (_cache.TryGetValue(cacheKey, out QrSessionState? state) && state?.IsApproved == true)
        {
            await Clients.Group($"QR_{sessionId}").SendAsync("QrLoginApproved", state.AuthResponse);
        }
    }

    // ─── Chat Room ───────────────────────────────────────────────────────────
    public async Task JoinChatRoom(string conversationId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, conversationId);
    }

    public async Task LeaveChatRoom(string conversationId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, conversationId);
    }

    public async Task SendMessage(SendMessageCommand command)
    {
        try
        {
            MessageDto messageDto = await _mediator.Send(command);
            await Clients.Group(command.ConversationId.ToString())
                .SendAsync("ReceiveNewMessage", messageDto);
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("ErrorMessage", ex.Message);
        }
    }

    public async Task ForwardMessage(ForwardMessageCommand command)
    {
        try
        {
            var dto = await _mediator.Send(command);
            await Clients.Group(dto.ConversationId.ToString())
                .SendAsync("ReceiveNewMessage", dto);
            await Clients.Caller.SendAsync("MessageForwarded", dto);
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("ErrorMessage", ex.Message);
        }
    }

    public async Task EditMessage(EditMessageCommand command)
    {
        try
        {
            var dto = await _mediator.Send(command);
            await Clients.Group(dto.ConversationId.ToString())
                .SendAsync("MessageEdited", dto);
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("ErrorMessage", ex.Message);
        }
    }

    public async Task DeleteMessage(DeleteMessageCommand command)
    {
        try
        {
            var dto = await _mediator.Send(command);
            await Clients.Group(dto.ConversationId.ToString())
                .SendAsync("MessageDeleted", dto);
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("ErrorMessage", ex.Message);
        }
    }

    public async Task PinMessage(PinMessageCommand command)
    {
        try
        {
            var dto = await _mediator.Send(command);
            await Clients.Group(dto.ConversationId.ToString())
                .SendAsync("MessagePinned", dto);
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("ErrorMessage", ex.Message);
        }
    }

    public async Task AddReaction(AddReactionCommand command)
    {
        try
        {
            var dto = await _mediator.Send(command);
            await Clients.Group(dto.ConversationId.ToString())
                .SendAsync("MessageReactionUpdated", dto);
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("ErrorMessage", ex.Message);
        }
    }

    public async Task RemoveReaction(RemoveReactionCommand command)
    {
        try
        {
            var dto = await _mediator.Send(command);
            await Clients.Group(dto.ConversationId.ToString())
                .SendAsync("MessageReactionUpdated", dto);
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("ErrorMessage", ex.Message);
        }
    }


    // ─── Call Signaling (WebRTC) ─────────────────────────────────────────────
    public async Task InitiateCall(ChatApp.Application.Features.Calls.Commands.CreateCall.CreateCallCommand command)
    {
        try
        {
            var callId = Guid.NewGuid();
            var createCmd = command with { Id = callId };
            await _mediator.Send(createCmd);

            await Clients.Caller.SendAsync("CallInitiated", callId);
            await Clients.OthersInGroup(command.ConversationId.ToString())
                .SendAsync("IncomingCall", new { callId, command.ConversationId, command.CallerId, command.Type });
        }
        catch (Exception ex) { await Clients.Caller.SendAsync("ErrorMessage", ex.Message); }
    }

    public async Task AcceptCall(Guid callId, string conversationId)
    {
        await _mediator.Send(new ChatApp.Application.Features.Calls.Commands.UpdateCallStatus.UpdateCallStatusCommand(callId, ChatApp.Domain.Enums.CallStatus.Ongoing));
        await Clients.OthersInGroup(conversationId).SendAsync("CallAccepted", callId);
    }

    public async Task RejectCall(Guid callId, string conversationId)
    {
        await _mediator.Send(new ChatApp.Application.Features.Calls.Commands.UpdateCallStatus.UpdateCallStatusCommand(callId, ChatApp.Domain.Enums.CallStatus.Rejected));
        await Clients.OthersInGroup(conversationId).SendAsync("CallRejected", callId);
    }

    public async Task EndCall(Guid callId, string conversationId)
    {
        await _mediator.Send(new ChatApp.Application.Features.Calls.Commands.UpdateCallStatus.UpdateCallStatusCommand(callId, ChatApp.Domain.Enums.CallStatus.Ended));
        await Clients.OthersInGroup(conversationId).SendAsync("CallEnded", callId);
    }

    public async Task SendWebRTCSignal(string conversationId, string payload)
    {
        await Clients.OthersInGroup(conversationId).SendAsync("ReceiveWebRTCSignal", payload);
    }
}
