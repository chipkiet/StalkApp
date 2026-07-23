using System;
using System.Threading.Tasks;
using ChatApp.Application.Features.Messages.Commands.AddReaction;
using ChatApp.Application.Features.Messages.Commands.DeleteMessage;
using ChatApp.Application.Features.Messages.Commands.EditMessage;
using ChatApp.Application.Features.Messages.Commands.ForwardMessage;
using ChatApp.Application.Features.Messages.Commands.PinMessage;
using ChatApp.Application.Features.Messages.Commands.RemoveReaction;
using ChatApp.Application.Features.Messages.Queries.GetMessages;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MessagesController : ControllerBase
{
    private readonly IMediator _mediator;

    public MessagesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{conversationId}/user/{userId}")]
    public async Task<IActionResult> GetMessages(Guid conversationId, Guid userId, [FromQuery] int count = 50)
    {
        var result = await _mediator.Send(new GetMessagesQuery(conversationId, userId, count));
        return Ok(result);
    }

    [HttpPut("{messageId}")]
    public async Task<IActionResult> EditMessage(Guid messageId, [FromBody] EditMessageRequest body)
    {
        try
        {
            var result = await _mediator.Send(new EditMessageCommand(messageId, body.UserId, body.NewContent));
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, ex.Message); }
        catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
    }

    [HttpDelete("{messageId}")]
    public async Task<IActionResult> DeleteMessage(Guid messageId, [FromQuery] Guid userId)
    {
        try
        {
            var result = await _mediator.Send(new DeleteMessageCommand(messageId, userId));
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, ex.Message); }
        catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
    }

    [HttpPost("{messageId}/pin")]
    public async Task<IActionResult> PinMessage(Guid messageId, [FromBody] PinMessageRequest body)
    {
        try
        {
            var result = await _mediator.Send(new PinMessageCommand(messageId, body.UserId, body.IsPinned));
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, ex.Message); }
        catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
    }

    [HttpPost("{messageId}/reactions")]
    public async Task<IActionResult> AddReaction(Guid messageId, [FromBody] AddReactionRequest body)
    {
        try
        {
            var result = await _mediator.Send(new AddReactionCommand(messageId, body.UserId, body.Emotion));
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, ex.Message); }
        catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
    }

    [HttpDelete("{messageId}/reactions")]
    public async Task<IActionResult> RemoveReaction(Guid messageId, [FromQuery] Guid userId)
    {
        try
        {
            var result = await _mediator.Send(new RemoveReactionCommand(messageId, userId));
            return Ok(result);
        }
        catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
    }

    [HttpPost("{messageId}/forward")]
    public async Task<IActionResult> ForwardMessage(Guid messageId, [FromBody] ForwardMessageRequest body)
    {
        try
        {
            var result = await _mediator.Send(new ForwardMessageCommand(messageId, body.UserId, body.TargetConversationId));
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, ex.Message); }
        catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
    }
}

public record EditMessageRequest(Guid UserId, string NewContent);
public record PinMessageRequest(Guid UserId, bool IsPinned);
public record AddReactionRequest(Guid UserId, string Emotion);
public record ForwardMessageRequest(Guid UserId, Guid TargetConversationId);
