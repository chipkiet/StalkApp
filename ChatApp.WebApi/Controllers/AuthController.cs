using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using ChatApp.Application.Features.Auth.Commands.RegisterUser;
using ChatApp.Shared.DTOs.Auth;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var command = new RegisterUserCommand
        {
            Email = request.Email,
            Password = request.Password,
            ConfirmPassword = request.ConfirmPassword,
            DisplayName = request.DisplayName
        };

        var response = await _mediator.Send(command);

        // Đăng nhập tự động bằng Cookie Auth
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, response.Id.ToString()),
            new Claim(ClaimTypes.Email, response.Email),
            new Claim(ClaimTypes.Name, response.DisplayName)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        return Ok(response);
    }
}
