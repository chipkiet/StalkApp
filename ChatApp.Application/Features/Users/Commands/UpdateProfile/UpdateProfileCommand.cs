using MediatR;

namespace ChatApp.Application.Features.Users.Commands.UpdateProfile;

/// <summary>
/// UC-03: Cập nhật thông tin cá nhân của user đã đăng nhập
/// </summary>
public record UpdateProfileCommand(
    Guid UserId,        // Lấy từ JWT Claims
    string DisplayName,
    string? Username,
    string? Bio,
    string? AvatarUrl
) : IRequest<UpdateProfileResult>;

public record UpdateProfileResult(
    bool Success,
    string Message,
    string? Username = null,
    string? DisplayName = null,
    string? AvatarUrl = null,
    string? Bio = null
);
