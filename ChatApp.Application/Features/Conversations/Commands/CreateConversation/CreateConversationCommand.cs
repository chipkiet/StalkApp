using System;
using System.Collections.Generic;
using ChatApp.Domain.Enums;
using MediatR;

namespace ChatApp.Application.Features.Conversations.Commands.CreateConversation;

public record CreateConversationCommand(
    Guid CreatorId,
    string? Title,
    ConversationType Type,
    List<Guid> ParticipantIds
) : IRequest<Guid>;
