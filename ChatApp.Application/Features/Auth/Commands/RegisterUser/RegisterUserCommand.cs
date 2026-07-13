using ChatApp.Shared.DTOs.Auth;
using MediatR;

namespace ChatApp.Application.Features.Auth.Commands.RegisterUser;

public class RegisterUserCommand : IRequest<RegisterResponse>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}
