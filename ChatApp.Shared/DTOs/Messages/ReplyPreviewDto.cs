using System;

namespace ChatApp.Application.DTOs.Messages;

public record ReplyPreviewDto(
    Guid MessageId,
    Guid SenderId,
    string? Content,
    bool IsDeleted,
    string? AttachmentName = null
);
