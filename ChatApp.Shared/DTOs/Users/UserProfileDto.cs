using System;
using ChatApp.Shared.Enums;

namespace ChatApp.Shared.DTOs.Users;

public class UserProfileDto
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string? Username { get; set; }
    public string? PhoneNumber { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
    public bool IsOnline { get; set; }
    public DateTime? LastSeenAt { get; set; }

    // Friendship info
    public FriendshipStatus? FriendshipStatus { get; set; }
    public bool IsRequester { get; set; }
}
