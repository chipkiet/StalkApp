using System;
using ChatApp.Domain.Enums;

namespace ChatApp.Domain.Entities;

public class ScheduledMessage
{
    public Guid Id { get; set; }
    public Guid ConversationId { get; set; }
    public Guid SenderId { get; set; }
    public string Content { get; set; } = null!;
    public DateTime ScheduledAt { get; set; }
    public bool IsSent { get; set; } = false;
    public DateTime CreatedAt { get; set; }
}
