using System;
using ChatApp.Domain.Enums;

namespace ChatApp.Domain.Entities;

public class Call
{
    public Guid Id { get; set; }
    public Guid ConversationId { get; set; }
    public Guid CallerId { get; set; }
    public CallType Type { get; set; }
    public CallStatus Status { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }

    // Navigation properties
    public Conversation Conversation { get; set; } = null!;
    public User Caller { get; set; } = null!;
}
