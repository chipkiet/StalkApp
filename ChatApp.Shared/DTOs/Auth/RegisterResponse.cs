using System;

namespace ChatApp.Shared.DTOs.Auth;

public class RegisterResponse
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}
