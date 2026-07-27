using ChatApp.Application.Features.Pinboard.Commands.CompleteCanvasTask;
using ChatApp.Application.Features.Pinboard.Commands.CreatePinboardConnection;
using ChatApp.Application.Features.Pinboard.Commands.CreatePinboardItem;
using ChatApp.Application.Features.Pinboard.Commands.DeletePinboardConnection;
using ChatApp.Application.Features.Pinboard.Commands.DeletePinboardItem;
using ChatApp.Application.Features.Pinboard.Commands.MovePinboardItem;
using ChatApp.Application.Features.Pinboard.Commands.UpdatePinboardItemContent;
using ChatApp.Application.Features.Pinboard.Queries.GetConnectionsByConversation;
using ChatApp.Application.Features.Pinboard.Queries.GetItemsByConversation;
using ChatApp.Shared.DTOs.Pinboard;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PinboardController : ControllerBase
{
    private readonly IMediator _mediator;

    public PinboardController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("conversation/{conversationId}")]
    public async Task<IActionResult> GetItems(Guid conversationId)
    {
        var items = await _mediator.Send(new GetItemsByConversationQuery(conversationId));
        return Ok(items);
    }

    [HttpPost]
    public async Task<IActionResult> CreateItem([FromBody] CreatePinboardItemCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPut("move")]
    public async Task<IActionResult> MoveItem([FromBody] MovePinboardItemCommand command)
    {
        var result = await _mediator.Send(command);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPut("complete-task")]
    public async Task<IActionResult> CompleteTask([FromBody] CompleteCanvasTaskCommand command)
    {
        var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value 
            ?? User.FindFirst("sub")?.Value 
            ?? User.FindFirst("nameid")?.Value;
            
        if (!Guid.TryParse(userIdStr, out Guid uid))
        {
            return Unauthorized();
        }

        command = command with { UserId = uid };

        var result = await _mediator.Send(command);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteItem(Guid id)
    {
        var result = await _mediator.Send(new DeletePinboardItemCommand(id));
        if (!result) return NotFound();
        return Ok();
    }

    [HttpPut("{id}/content")]
    public async Task<IActionResult> UpdateContent(Guid id, [FromBody] UpdateContentRequest request)
    {
        var result = await _mediator.Send(new UpdatePinboardItemContentCommand(id, request.Content));
        if (!result) return NotFound();
        return Ok(new { success = true });
    }

    [HttpGet("{conversationId}/connections")]
    public async Task<ActionResult<List<PinboardConnectionDto>>> GetConnections(Guid conversationId)
    {
        var items = await _mediator.Send(new GetConnectionsByConversationQuery(conversationId));
        return Ok(items);
    }

    [HttpPost("{conversationId}/connections")]
    public async Task<ActionResult<PinboardConnectionDto>> CreateConnection(Guid conversationId, [FromBody] CreatePinboardConnectionCommand command)
    {
        var item = await _mediator.Send(command with { ConversationId = conversationId });
        return Ok(item);
    }

    [HttpDelete("connections/{id}")]
    public async Task<IActionResult> DeleteConnection(Guid id)
    {
        var result = await _mediator.Send(new DeletePinboardConnectionCommand(id));
        if (!result) return NotFound();
        return Ok(new { success = true });
    }
}

public record UpdateContentRequest(string Content);
