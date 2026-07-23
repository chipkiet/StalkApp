namespace ChatApp.Shared.DTOs.Users;

public class UpdateProfileRequest
{
    /// <summary>
    /// Tên hiển thị (bắt buộc)
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Tên người dùng (username, không bắt buộc nếu không đổi)
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Mô tả giới thiệu bản thân
    /// </summary>
    public string? Bio { get; set; }

    /// <summary>
    /// URL ảnh đại diện
    /// </summary>
    public string? AvatarUrl { get; set; }
}
