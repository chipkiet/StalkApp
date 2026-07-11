using System;
using System.Threading.Tasks;
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

    [HttpGet("{conversationId}")]
    public async Task<IActionResult> GetMessages(Guid conversationId, [FromQuery] int count = 50)
    {
        var result = await _mediator.Send(new GetMessagesQuery(conversationId, count));
        return Ok(result);
    }
}
