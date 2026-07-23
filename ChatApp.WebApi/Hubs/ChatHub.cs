using System;
using System.Threading.Tasks;
using ChatApp.Application.DTOs.Messages;
using ChatApp.Application.Features.Messages.Commands.AddReaction;
using ChatApp.Application.Features.Messages.Commands.DeleteMessage;
using ChatApp.Application.Features.Messages.Commands.EditMessage;
using ChatApp.Application.Features.Messages.Commands.ForwardMessage;
using ChatApp.Application.Features.Messages.Commands.PinMessage;
using ChatApp.Application.Features.Messages.Commands.RemoveReaction;
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
