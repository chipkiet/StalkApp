using System;
using System.Collections.Generic;
using ChatApp.Domain.Enums;

namespace ChatApp.Domain.Entities;

public class Message
{
    public Guid Id { get; set; }
    public Guid ConversationId { get; set; }
    public Guid SenderId { get; set; }
    public MessageType MessageType { get; set; }
    public string? Content { get; set; }
    public Guid? ReplyToMessageId { get; set; }
    public Guid? ForwardedFromMessageId { get; set; }
    public bool IsPinned { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public Conversation Conversation { get; set; } = null!;
    public User Sender { get; set; } = null!;
    
    public Message? ReplyToMessage { get; set; }
    public ICollection<Message> Replies { get; set; } = new List<Message>();

    public Message? ForwardedFromMessage { get; set; }
    public ICollection<Message> ForwardedMessages { get; set; } = new List<Message>();

    public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
    public ICollection<MessageReaction> Reactions { get; set; } = new List<MessageReaction>();
    public ICollection<PinboardItem> LinkedPinboardItems { get; set; } = new List<PinboardItem>();
}
