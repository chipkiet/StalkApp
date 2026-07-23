using System.Security.Claims;
using ChatApp.Domain.Entities;

namespace ChatApp.Application.Interfaces.Services;

public interface ITokenService
{
    /// <summary>
    /// Tạo JWT Access Token từ thông tin User
    /// </summary>
    string GenerateAccessToken(User user);

    /// <summary>
    /// Tạo Refresh Token ngẫu nhiên (opaque string)
    /// </summary>
    string GenerateRefreshToken();

    /// <summary>
    /// Đọc ClaimsPrincipal từ một Access Token đã hết hạn (dùng để refresh)
    /// </summary>
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);

    /// <summary>
    /// Thời gian hết hạn của Access Token (phút)
    /// </summary>
    int AccessTokenExpiryMinutes { get; }
}
