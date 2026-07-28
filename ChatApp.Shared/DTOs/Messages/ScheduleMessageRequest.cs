namespace ChatApp.Shared.DTOs.Messages;
using System;

public class ScheduleMessageRequest
{
    public Guid ConversationId { get; set; }
    public string Content { get; set; } = null!;
    public DateTime ScheduledAt { get; set; }
}
