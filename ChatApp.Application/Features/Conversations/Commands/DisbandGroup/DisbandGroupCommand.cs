using System;
using MediatR;

namespace ChatApp.Application.Features.Conversations.Commands.DisbandGroup;

public record DisbandGroupCommand(
    Guid ConversationId,
    Guid RequesterId
) : IRequest;
