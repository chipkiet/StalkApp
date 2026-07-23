using System;
using System.Collections.Generic;
using ChatApp.Domain.Enums;

namespace ChatApp.Application.DTOs.Messages;

public record MessageDto(
    Guid Id,
    Guid ConversationId,
    Guid SenderId,
    MessageType MessageType,
    string? Content,
    DateTime CreatedAt,
    string? AttachmentUrl = null,
    string? AttachmentName = null,
    bool IsPinned = false,
    bool IsDeleted = false,
    DateTime? UpdatedAt = null,
    IReadOnlyList<ReactionDto>? Reactions = null,
    Guid? ReplyToMessageId = null,
    ReplyPreviewDto? ReplyTo = null,
    Guid? ForwardedFromMessageId = null,
    bool IsForwarded = false
);
