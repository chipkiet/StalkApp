using System;
using System.Threading.Tasks;
using ChatApp.Application.Features.Conversations.Queries.GetInbox;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConversationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ConversationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("inbox/{userId}")]
    public async Task<IActionResult> GetInbox(Guid userId)
    {
        var result = await _mediator.Send(new GetInboxQuery(userId));
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateConversation([FromBody] ChatApp.Application.Features.Conversations.Commands.CreateConversation.CreateConversationCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{conversationId}/user/{userId}")]
    public async Task<IActionResult> DeleteConversation(Guid conversationId, Guid userId)
    {
        var command = new ChatApp.Application.Features.Conversations.Commands.DeleteConversation.DeleteConversationCommand(conversationId, userId);
        await _mediator.Send(command);
        return Ok();
    }

    [HttpGet("{id}/participants")]
    public async Task<IActionResult> GetParticipants(Guid id, [FromServices] ChatApp.Application.Interfaces.Repositories.IGenericRepository<ChatApp.Domain.Entities.Participant> participantRepo, [FromServices] ChatApp.Application.Interfaces.Repositories.IGenericRepository<ChatApp.Domain.Entities.User> userRepo)
    {
        var participants = await participantRepo.FindAsync(p => p.ConversationId == id);
        var result = new System.Collections.Generic.List<object>();
        foreach(var p in participants)
        {
            var user = await userRepo.GetByIdAsync(p.UserId);
            if (user != null)
            {
                result.Add(new {
                    userId = user.Id,
                    displayName = user.DisplayName,
                    phoneNumber = user.PhoneNumber,
                    avatarUrl = user.AvatarUrl,
                    bio = user.Bio
                });
            }
        }
        return Ok(result);
    }

    [HttpPost("{conversationId}/pin")]
    public async Task<IActionResult> TogglePinConversation(Guid conversationId, [FromBody] TogglePinConversationRequest body)
    {
        try
        {
            var command = new ChatApp.Application.Features.Conversations.Commands.TogglePinConversation.TogglePinConversationCommand(conversationId, body.UserId, body.IsPinned);
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, ex.Message); }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    [HttpPut("{id}/mute")]
    public async Task<IActionResult> ToggleMute(Guid id, [FromQuery] Guid userId, [FromQuery] bool isMuted)
    {
        await _mediator.Send(new ChatApp.Application.Features.Conversations.Commands.ToggleMute.ToggleMuteCommand(id, userId, isMuted));
        return Ok();
    }
}

public record TogglePinConversationRequest(Guid UserId, bool IsPinned);
