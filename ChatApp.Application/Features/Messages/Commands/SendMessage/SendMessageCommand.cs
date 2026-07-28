using System;
using ChatApp.Application.DTOs.Messages;
using ChatApp.Domain.Enums;
using MediatR;

namespace ChatApp.Application.Features.Messages.Commands.SendMessage;

public record SendMessageCommand(
    Guid ConversationId,
    Guid SenderId,
    MessageType MessageType,
    string Content,
    string? AttachmentUrl = null,
    string? AttachmentName = null,
    string? AttachmentContentType = null,
    long? AttachmentSize = null,
    Guid? ReplyToMessageId = null,
    List<Guid>? MentionedUserIds = null
) : IRequest<MessageDto>;
