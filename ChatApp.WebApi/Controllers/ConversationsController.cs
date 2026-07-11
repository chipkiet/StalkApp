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
}
