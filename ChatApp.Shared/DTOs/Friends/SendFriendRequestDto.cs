namespace ChatApp.Shared.DTOs.Friends;

/// <summary>
/// Request body khi gửi lời mời kết bạn
/// </summary>
public class SendFriendRequestDto
{
    public Guid AddresseeId { get; set; }
}
