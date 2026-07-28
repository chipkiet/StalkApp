using System;
using System.Security.Claims;
using System.Threading.Tasks;
using ChatApp.Application.Features.Conversations.Queries.GetInbox;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ConversationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ConversationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value
            ?? User.FindFirst("nameid")?.Value;
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }

    [HttpGet("inbox")]
    public async Task<IActionResult> GetInbox()
    {
        var userId = GetCurrentUserId();
        var result = await _mediator.Send(new GetInboxQuery(userId));
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateConversation([FromBody] ChatApp.Application.Features.Conversations.Commands.CreateConversation.CreateConversationCommand command)
    {
        var userId = GetCurrentUserId();
        var cmdWithCreator = command with { CreatorId = userId };
        var result = await _mediator.Send(cmdWithCreator);
        return Ok(result);
    }

    [HttpDelete("{conversationId}")]
    public async Task<IActionResult> DeleteConversation(Guid conversationId)
    {
        var userId = GetCurrentUserId();
        var command = new ChatApp.Application.Features.Conversations.Commands.DeleteConversation.DeleteConversationCommand(conversationId, userId);
        try
        {
            await _mediator.Send(command);
            return Ok();
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { message = ex.Message }); }
    }

    [HttpGet("{id}/participants")]
    public async Task<IActionResult> GetParticipants(Guid id, [FromServices] ChatApp.Application.Interfaces.Repositories.IGenericRepository<ChatApp.Domain.Entities.Participant> participantRepo, [FromServices] ChatApp.Application.Interfaces.Repositories.IGenericRepository<ChatApp.Domain.Entities.User> userRepo)
    {
        // Kiểm tra quyền (phải là participant mới được xem danh sách)
        var userId = GetCurrentUserId();
        var isParticipant = (await participantRepo.FindAsync(p => p.ConversationId == id && p.UserId == userId)).Any();
        if (!isParticipant)
        {
            return StatusCode(403, new { message = "Bạn không có quyền truy cập thông tin nhóm này." });
        }

        var participants = await participantRepo.FindAsync(p => p.ConversationId == id);
        var result = new System.Collections.Generic.List<object>();
        foreach (var p in participants)
        {
            var user = await userRepo.GetByIdAsync(p.UserId);
            if (user != null)
            {
                result.Add(new
                {
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
            var userId = GetCurrentUserId();
            var command = new ChatApp.Application.Features.Conversations.Commands.TogglePinConversation.TogglePinConversationCommand(conversationId, userId, body.IsPinned);
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { message = ex.Message }); }
        catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPut("{id}/mute")]
    public async Task<IActionResult> ToggleMute(Guid id, [FromQuery] bool isMuted)
    {
        var userId = GetCurrentUserId();
        try
        {
            await _mediator.Send(new ChatApp.Application.Features.Conversations.Commands.ToggleMute.ToggleMuteCommand(id, userId, isMuted));
            return Ok();
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { message = ex.Message }); }
        catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
    }
}

public record TogglePinConversationRequest(bool IsPinned);
