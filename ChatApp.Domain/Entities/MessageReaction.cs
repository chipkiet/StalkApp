using System;

namespace ChatApp.Domain.Entities;

public class MessageReaction
{
    public Guid MessageId { get; set; }
    public Guid UserId { get; set; }
    public string Emotion { get; set; } = string.Empty;

    // Navigation properties
    public Message Message { get; set; } = null!;
    public User User { get; set; } = null!;
}
