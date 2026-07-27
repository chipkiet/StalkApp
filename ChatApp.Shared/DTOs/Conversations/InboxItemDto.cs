using System;
using ChatApp.Domain.Enums;

namespace ChatApp.Application.DTOs.Conversations;

public record InboxItemDto(
    Guid ConversationId,
    string? Title,
    string? AvatarUrl,
    ConversationType Type,
    string LastMessage,
    DateTime? LastMessageAt,
    int UnreadCount,
    bool IsMuted = false
);
