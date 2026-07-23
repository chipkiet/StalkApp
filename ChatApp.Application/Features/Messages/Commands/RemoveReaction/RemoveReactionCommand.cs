using System;
using ChatApp.Application.DTOs.Messages;
using MediatR;

namespace ChatApp.Application.Features.Messages.Commands.RemoveReaction;

public record RemoveReactionCommand(
    Guid MessageId,
    Guid UserId
) : IRequest<MessageDto>;
