using ChatApp.Shared.Enums;

namespace ChatApp.Shared.DTOs.Friends;

/// <summary>
/// DTO trả về khi hiển thị một người trong danh sách bạn bè hoặc lời mời kết bạn.
/// </summary>
public class FriendDto
{
    public Guid FriendshipId { get; set; }
    public Guid UserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string? Username { get; set; }
    public string? AvatarUrl { get; set; }
    public string? PhoneNumber { get; set; }
    public bool IsOnline { get; set; }
    public DateTime? LastSeenAt { get; set; }
    public FriendshipStatus Status { get; set; }

    /// <summary>Mình (current user) là người đã gửi lời mời</summary>
    public bool IsRequester { get; set; }
}
