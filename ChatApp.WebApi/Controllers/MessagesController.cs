using System;
using System.Security.Claims;
using System.Threading.Tasks;
using ChatApp.Application.Features.Messages.Commands.AddReaction;
using ChatApp.Application.Features.Messages.Commands.DeleteMessage;
using ChatApp.Application.Features.Messages.Commands.EditMessage;
using ChatApp.Application.Features.Messages.Commands.ForwardMessage;
using ChatApp.Application.Features.Messages.Commands.PinMessage;
using ChatApp.Application.Features.Messages.Commands.RemoveReaction;
using ChatApp.Application.Features.Messages.Queries.GetMessages;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MessagesController : ControllerBase
{
    private readonly IMediator _mediator;

    public MessagesController(IMediator mediator)
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

    [HttpGet("{conversationId}")]
    public async Task<IActionResult> GetMessages(Guid conversationId, [FromQuery] int count = 50)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _mediator.Send(new GetMessagesQuery(conversationId, userId, count));
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { message = ex.Message }); }
    }

    [HttpPut("{messageId}")]
    public async Task<IActionResult> EditMessage(Guid messageId, [FromBody] EditMessageRequest body)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _mediator.Send(new EditMessageCommand(messageId, userId, body.NewContent));
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpDelete("{messageId}")]
    public async Task<IActionResult> DeleteMessage(Guid messageId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _mediator.Send(new DeleteMessageCommand(messageId, userId));
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPost("{messageId}/pin")]
    public async Task<IActionResult> PinMessage(Guid messageId, [FromBody] PinMessageRequest body)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _mediator.Send(new PinMessageCommand(messageId, userId, body.IsPinned));
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPost("{messageId}/reactions")]
    public async Task<IActionResult> AddReaction(Guid messageId, [FromBody] AddReactionRequest body)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _mediator.Send(new AddReactionCommand(messageId, userId, body.Emotion));
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpDelete("{messageId}/reactions")]
    public async Task<IActionResult> RemoveReaction(Guid messageId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _mediator.Send(new RemoveReactionCommand(messageId, userId));
            return Ok(result);
        }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPost("{messageId}/forward")]
    public async Task<IActionResult> ForwardMessage(Guid messageId, [FromBody] ForwardMessageRequest body)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _mediator.Send(new ForwardMessageCommand(messageId, userId, body.TargetConversationId));
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPost("schedule")]
    public async Task<IActionResult> ScheduleMessage([FromBody] ChatApp.Shared.DTOs.Messages.ScheduleMessageRequest body)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _mediator.Send(new ChatApp.Application.Features.Messages.Commands.ScheduleMessage.CreateScheduledMessageCommand(body.ConversationId, userId, body.Content, body.ScheduledAt));
            return Ok(new { id = result });
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }
}

public record EditMessageRequest(string NewContent);
public record PinMessageRequest(bool IsPinned);
public record AddReactionRequest(string Emotion);
public record ForwardMessageRequest(Guid TargetConversationId);
