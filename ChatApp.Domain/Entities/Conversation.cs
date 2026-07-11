using System;
using System.Collections.Generic;
using ChatApp.Domain.Enums;

namespace ChatApp.Domain.Entities;

public class Conversation
{
    public Guid Id { get; set; }
    public string? Title { get; set; }
    public string? AvatarUrl { get; set; }
    public ConversationType Type { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public ICollection<Participant> Participants { get; set; } = new List<Participant>();
    public ICollection<Message> Messages { get; set; } = new List<Message>();
    public ICollection<Call> Calls { get; set; } = new List<Call>();
}
