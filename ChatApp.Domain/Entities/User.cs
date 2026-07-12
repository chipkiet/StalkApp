using System;
using System.Collections.Generic;

namespace ChatApp.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Bio { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
    public bool IsOnline { get; set; }
    public DateTime? LastSeenAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<Participant> Participants { get; set; } = new List<Participant>();
    public ICollection<Message> Messages { get; set; } = new List<Message>();
    public ICollection<MessageReaction> MessageReactions { get; set; } = new List<MessageReaction>();
    public ICollection<Call> CallsInitiated { get; set; } = new List<Call>();
}
