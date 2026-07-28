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
using ChatApp.Application.Features.Pinboard.Commands.CreatePinboardItem;
using ChatApp.Application.Features.Pinboard.Commands.MovePinboardItem;
using ChatApp.Application.Features.Pinboard.Commands.CompleteCanvasTask;
using ChatApp.Application.Features.Pinboard.Commands.DeletePinboardItem;
using ChatApp.Application.Features.Pinboard.Commands.UpdatePinboardItemContent;
using ChatApp.Application.Features.Pinboard.Commands.CreatePinboardConnection;
using ChatApp.Application.Features.Pinboard.Commands.DeletePinboardConnection;
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
    private readonly IGenericRepository<Participant> _participantRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMemoryCache _cache;

    public ChatHub(
        IMediator mediator,
        IPresenceTracker presenceTracker,
        IGenericRepository<User> userRepository,
        IGenericRepository<Participant> participantRepository,
        IUnitOfWork unitOfWork,
        IMemoryCache cache)
    {
        _mediator = mediator;
        _presenceTracker = presenceTracker;
        _userRepository = userRepository;
        _participantRepository = participantRepository;
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

        // ← Thêm vào user-group riêng để nhận IncomingCall dù chưa vào conversation room
        await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");

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

            // Gửi IncomingCall qua conversation group (nếu đã join)
            await Clients.OthersInGroup(command.ConversationId.ToString())
                .SendAsync("IncomingCall", callId, command.ConversationId, command.CallerId, (int)command.Type);

            // ← QUAN TRỌNG: Cũng gửi qua user-group riêng của từng participant
            // để đảm bảo họ nhận được dù chưa mở conversation đó
            var participants = await _participantRepository.FindAsync(p => p.ConversationId == command.ConversationId && p.UserId != command.CallerId);

            foreach (var participant in participants)
            {
                await Clients.Group($"user_{participant.UserId}")
                    .SendAsync("IncomingCall", callId, command.ConversationId, command.CallerId, (int)command.Type);
            }
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

        var msgCmd = new SendMessageCommand(
            Guid.Parse(conversationId),
            GetCurrentUserId(),
            ChatApp.Domain.Enums.MessageType.CallMissed,
            "Cuộc gọi bị từ chối."
        );
        var dto = await _mediator.Send(msgCmd);
        await Clients.Group(conversationId).SendAsync("ReceiveNewMessage", dto);
    }

    public async Task EndCall(Guid callId, string conversationId)
    {
        await _mediator.Send(new ChatApp.Application.Features.Calls.Commands.UpdateCallStatus.UpdateCallStatusCommand(callId, ChatApp.Domain.Enums.CallStatus.Ended));
        await Clients.OthersInGroup(conversationId).SendAsync("CallEnded", callId);

        var msgCmd = new SendMessageCommand(
            Guid.Parse(conversationId),
            GetCurrentUserId(),
            ChatApp.Domain.Enums.MessageType.CallEnded,
            "Cuộc gọi đã kết thúc."
        );
        var dto = await _mediator.Send(msgCmd);
        await Clients.Group(conversationId).SendAsync("ReceiveNewMessage", dto);
    }

    public async Task SendWebRTCSignal(string conversationId, string payload)
    {
        await Clients.OthersInGroup(conversationId).SendAsync("ReceiveWebRTCSignal", payload);
    }

    // ─── Gamified Pinboard & Canvas ──────────────────────────────────────────
    public async Task CreatePinboardItem(CreatePinboardItemCommand command)
    {
        try
        {
            var dto = await _mediator.Send(command);
            await Clients.Group(command.ConversationId.ToString())
                .SendAsync("PinboardItemCreated", dto);
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("ErrorMessage", ex.Message);
        }
    }

    public async Task MovePinboardItem(MovePinboardItemCommand command, string conversationId)
    {
        try
        {
            var dto = await _mediator.Send(command);
            if (dto != null)
            {
                // Đồng bộ tọa độ cho tất cả (trừ người kéo để tránh giật lag client)
                await Clients.OthersInGroup(conversationId)
                    .SendAsync("PinboardItemMoved", dto);
            }
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("ErrorMessage", ex.Message);
        }
    }

    public async Task CompleteCanvasTask(CompleteCanvasTaskCommand command, string conversationId)
    {
        try
        {
            var dto = await _mediator.Send(command);
            if (dto != null)
            {
                // Bắn pháo hoa (Gamification) và cập nhật điểm Karma cho toàn nhóm
                await Clients.Group(conversationId)
                    .SendAsync("CanvasTaskCompleted", dto, command.TaskId);
            }
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("ErrorMessage", ex.Message);
        }
    }

    public async Task DeletePinboardItem(DeletePinboardItemCommand command, string conversationId)
    {
        try
        {
            var result = await _mediator.Send(command);
            if (result)
            {
                await Clients.Group(conversationId).SendAsync("PinboardItemDeleted", command.Id);
            }
        }
        catch (Exception ex) { await Clients.Caller.SendAsync("ErrorMessage", ex.Message); }
    }

    public async Task UpdatePinboardItemContent(UpdatePinboardItemContentCommand command, string conversationId)
    {
        try
        {
            var result = await _mediator.Send(command);
            if (result)
            {
                await Clients.OthersInGroup(conversationId).SendAsync("PinboardItemUpdated", command);
            }
        }
        catch (Exception ex) { await Clients.Caller.SendAsync("ErrorMessage", ex.Message); }
    }

    public async Task CreatePinboardConnection(CreatePinboardConnectionCommand command)
    {
        try
        {
            var dto = await _mediator.Send(command);
            await Clients.Group(dto.ConversationId.ToString()).SendAsync("PinboardConnectionCreated", dto);
        }
        catch (Exception ex) { await Clients.Caller.SendAsync("ErrorMessage", ex.Message); }
    }

    public async Task DeletePinboardConnection(DeletePinboardConnectionCommand command, string conversationId)
    {
        try
        {
            var result = await _mediator.Send(command);
            if (result)
            {
                await Clients.Group(conversationId).SendAsync("PinboardConnectionDeleted", command.Id);
            }
        }
        catch (Exception ex) { await Clients.Caller.SendAsync("ErrorMessage", ex.Message); }
    }

    public async Task MovePinboardItemLive(string itemId, double x, double y, string conversationId)
    {
        if (Guid.TryParse(itemId, out var parsedItemId))
        {
            await Clients.GroupExcept(conversationId, Context.ConnectionId)
                         .SendAsync("PinboardItemMovedLive", parsedItemId, x, y);
        }
    }

    public async Task MoveCursor(string conversationId, double x, double y)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return;

        // Broadcast cursor position to others in the same room
        // Throttle frequency is handled by the client
        await Clients.OthersInGroup(conversationId).SendAsync("CursorMoved", userId, x, y);
    }
}

