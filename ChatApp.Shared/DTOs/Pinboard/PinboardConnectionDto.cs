using System;

namespace ChatApp.Shared.DTOs.Pinboard;

public class PinboardConnectionDto
{
    public Guid Id { get; set; }
    public Guid ConversationId { get; set; }
    public Guid SourceItemId { get; set; }
    public Guid TargetItemId { get; set; }
    public string? Label { get; set; }
}
