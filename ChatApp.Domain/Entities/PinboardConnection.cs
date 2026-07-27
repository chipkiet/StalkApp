using System;

namespace ChatApp.Domain.Entities;

public class PinboardConnection
{
    public Guid Id { get; set; }
    public Guid ConversationId { get; set; }
    
    public Guid SourceItemId { get; set; }
    public PinboardItem SourceItem { get; set; } = null!;
    
    public Guid TargetItemId { get; set; }
    public PinboardItem TargetItem { get; set; } = null!;

    public string? Label { get; set; }
    public DateTime CreatedAt { get; set; }
}
