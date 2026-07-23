using System;
using ChatApp.Application.DTOs.Messages;
using MediatR;

namespace ChatApp.Application.Features.Messages.Commands.DeleteMessage;

/// <summary>Soft-delete a message for everyone in the conversation.</summary>
public record DeleteMessageCommand(
    Guid MessageId,
    Guid UserId
) : IRequest<MessageDto>;
