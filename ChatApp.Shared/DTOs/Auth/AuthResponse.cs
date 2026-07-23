namespace ChatApp.Shared.DTOs.Auth;

public class AuthResponse
{
    /// <summary>
    /// JWT Access Token (ngắn hạn, dùng để gọi API)
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Refresh Token (dài hạn, dùng để gia hạn Access Token)
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// Thời điểm hết hạn của Access Token (UTC)
    /// </summary>
    public DateTime AccessTokenExpiry { get; set; }

    // Thông tin cơ bản của User
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// Đây là user mới đăng ký lần đầu không? (phân biệt UC-01 vs UC-02)
    /// </summary>
    public bool IsNewUser { get; set; }
}
