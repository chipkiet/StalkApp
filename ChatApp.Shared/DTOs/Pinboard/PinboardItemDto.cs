using System;
using ChatApp.Shared.Enums;

namespace ChatApp.Shared.DTOs.Pinboard
{
    public class PinboardItemDto
    {
        public Guid Id { get; set; }
        public Guid ConversationId { get; set; }
        public Guid? LinkedMessageId { get; set; }
        public PinboardItemType Type { get; set; }
        public string? Content { get; set; }
        
        public double PositionX { get; set; }
        public double PositionY { get; set; }
        public int ZIndex { get; set; }

        public Guid? AssignedToUserId { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? Deadline { get; set; }
        
        public DateTime CreatedAt { get; set; }
    }
}
