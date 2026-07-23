using System;
using ChatApp.Application.DTOs.Messages;
using MediatR;

namespace ChatApp.Application.Features.Messages.Commands.AddReaction;

public record AddReactionCommand(
    Guid MessageId,
    Guid UserId,
    string Emotion
) : IRequest<MessageDto>;
