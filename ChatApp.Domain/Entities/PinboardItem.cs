using System;
using ChatApp.Shared.Enums;

namespace ChatApp.Domain.Entities
{
    public class PinboardItem
    {
        public Guid Id { get; set; }
        public Guid ConversationId { get; set; }
        public Conversation Conversation { get; set; }

        public Guid? LinkedMessageId { get; set; }
        public Message LinkedMessage { get; set; }

        public PinboardItemType Type { get; set; }
        public string Content { get; set; }

        // Position coordinates (px)
        public double PositionX { get; set; }
        public double PositionY { get; set; }
        public int ZIndex { get; set; }

        // Task specific properties (nullable if it's just a StickyNote)
        public Guid? AssignedToUserId { get; set; }
        public User AssignedToUser { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? Deadline { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
