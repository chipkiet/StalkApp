using System;
using ChatApp.Domain.Enums;

namespace ChatApp.Domain.Entities;

public class Participant
{
    public Guid ConversationId { get; set; }
    public Guid UserId { get; set; }
    public ParticipantRole Role { get; set; }
    public Guid? LastReadMessageId { get; set; }
    public DateTime JoinedAt { get; set; }

    // Navigation properties
    public Conversation Conversation { get; set; } = null!;
    public User User { get; set; } = null!;
    public Message? LastReadMessage { get; set; }
}
