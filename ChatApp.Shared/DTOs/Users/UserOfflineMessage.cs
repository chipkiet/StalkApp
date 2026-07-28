using System;

namespace ChatApp.Shared.DTOs.Users;

public class UserOfflineMessage
{
    public Guid UserId { get; set; }
    public DateTime LastSeenAt { get; set; }
}
