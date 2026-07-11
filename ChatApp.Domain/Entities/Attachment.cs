using System;

namespace ChatApp.Domain.Entities;

public class Attachment
{
    public Guid Id { get; set; }
    public Guid MessageId { get; set; }
    public string FileUrl { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }

    // Navigation properties
    public Message Message { get; set; } = null!;
}
