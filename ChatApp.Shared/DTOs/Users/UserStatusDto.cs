namespace ChatApp.Shared.DTOs.Users;

public class UserStatusDto
{
    public Guid UserId { get; set; }
    public bool IsOnline { get; set; }

    /// <summary>
    /// Lần cuối online (UTC). Null nếu đang online.
    /// </summary>
    public DateTime? LastSeenAt { get; set; }
}
