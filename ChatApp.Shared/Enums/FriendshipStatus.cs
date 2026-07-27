namespace ChatApp.Shared.Enums;

public enum FriendshipStatus
{
    /// <summary>Lời mời đang chờ phản hồi</summary>
    Pending = 0,

    /// <summary>Đã chấp nhận, hai bên là bạn bè</summary>
    Accepted = 1,

    /// <summary>Bị chặn</summary>
    Blocked = 2
}
