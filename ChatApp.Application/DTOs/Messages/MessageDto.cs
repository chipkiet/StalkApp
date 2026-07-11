using System;
using ChatApp.Domain.Enums;

namespace ChatApp.Application.DTOs.Messages;

public record MessageDto(
    Guid Id,
    Guid ConversationId,
    Guid SenderId,
    MessageType MessageType,
    string Content,
    DateTime CreatedAt,
    string? AttachmentUrl = null,
    string? AttachmentName = null
);
