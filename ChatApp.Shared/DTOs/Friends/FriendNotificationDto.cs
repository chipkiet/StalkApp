namespace ChatApp.Shared.DTOs.Friends;

/// <summary>
/// DTO dùng cho SignalR events liên quan đến kết bạn.
/// Áp dụng cho: FriendRequestReceived, FriendRequestAccepted, FriendRequestDeclined, FriendRemoved
/// </summary>
public class FriendNotificationDto
{
    public Guid FriendshipId { get; set; }

    /// <summary>UserId của người liên quan (người gửi hoặc người chấp nhận)</summary>
    public Guid UserId { get; set; }

    public string DisplayName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
}
