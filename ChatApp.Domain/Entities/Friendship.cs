using ChatApp.Shared.Enums;

namespace ChatApp.Domain.Entities;

/// <summary>
/// Đại diện cho mối quan hệ kết bạn giữa hai người dùng.
/// Status = Pending  → Lời mời chưa được phản hồi
/// Status = Accepted → Đã là bạn bè
/// Status = Blocked  → Bị chặn
/// </summary>
public class Friendship
{
    public Guid Id { get; set; }

    /// <summary>Người gửi lời mời kết bạn</summary>
    public Guid RequesterId { get; set; }

    /// <summary>Người nhận lời mời kết bạn</summary>
    public Guid AddresseeId { get; set; }

    public FriendshipStatus Status { get; set; } = FriendshipStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public User Requester { get; set; } = null!;
    public User Addressee { get; set; } = null!;
}
