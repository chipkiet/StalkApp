using System;
using ChatApp.Application.DTOs.Messages;
using MediatR;

namespace ChatApp.Application.Features.Messages.Commands.ForwardMessage;

public record ForwardMessageCommand(
    Guid MessageId,
    Guid SenderId,
    Guid TargetConversationId
) : IRequest<MessageDto>;
