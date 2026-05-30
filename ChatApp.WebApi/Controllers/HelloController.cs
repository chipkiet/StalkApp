using ChatApp.Application.Features.Hello.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HelloController : ControllerBase
{
    private readonly IMediator _mediator;

    // Inject MediatR vào Controller
    public HelloController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<string>> SayHello()
    {
        // Bắn Query vào hệ thống MediatR, nó sẽ tự tìm đến GetHelloQueryHandler để xử lý
        var result = await _mediator.Send(new GetHelloQueries());
        
        return Ok(result);
    }
}