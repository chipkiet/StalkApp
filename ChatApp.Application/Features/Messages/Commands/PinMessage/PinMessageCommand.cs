using System;
using ChatApp.Application.DTOs.Messages;
using MediatR;

namespace ChatApp.Application.Features.Messages.Commands.PinMessage;

public record PinMessageCommand(
    Guid MessageId,
    Guid UserId,
    bool IsPinned
) : IRequest<MessageDto>;
