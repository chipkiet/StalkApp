using System;
using System.Threading.Tasks;
using ChatApp.Application.DTOs.Messages;
using ChatApp.Application.Features.Messages.Commands.SendMessage;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace ChatApp.WebApi.Hubs;

public class ChatHub : Hub
{
    private readonly IMediator _mediator;

    public ChatHub(IMediator mediator)
    {
        _mediator = mediator;
    }

    // Khi User click vào phòng chat, họ sẽ join vào Group để nhận tin
    public async Task JoinChatRoom(string conversationId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, conversationId);
    }

    // Khi User rời phòng chat
    public async Task LeaveChatRoom(string conversationId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, conversationId);
    }

    // Client gọi hàm này để gửi tin nhắn
    public async Task SendMessage(SendMessageCommand command)
    {
        try
        {
            // 1. Gửi Command vào MediatR để lưu Database
            MessageDto messageDto = await _mediator.Send(command);

            // 2. Sau khi lưu thành công, Broadcast tin nhắn này tới những người đang mở phòng Chat
            await Clients.Group(command.ConversationId.ToString())
                .SendAsync("ReceiveNewMessage", messageDto);
        }
        catch (Exception ex)
        {
            // Báo lỗi về lại cho người gửi
            await Clients.Caller.SendAsync("ErrorMessage", ex.Message);
        }
    }

    // Luồng Báo hiệu Cuộc gọi (Call Signaling)
    public async Task InitiateCall(ChatApp.Application.Features.Calls.Commands.CreateCall.CreateCallCommand command)
    {
        try
        {
            var callId = Guid.NewGuid();
            var createCmd = command with { Id = callId };
            await _mediator.Send(createCmd);

            // Báo lại cho Caller biết ID cuộc gọi
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
        // Gửi thông tin SDP / ICE Candidates cho người kia
        await Clients.OthersInGroup(conversationId).SendAsync("ReceiveWebRTCSignal", payload);
    }
}
