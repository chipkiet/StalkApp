using System;
using ChatApp.Application.DTOs.Messages;
using MediatR;

namespace ChatApp.Application.Features.Messages.Commands.EditMessage;

public record EditMessageCommand(
    Guid MessageId,
    Guid UserId,
    string NewContent
) : IRequest<MessageDto>;
