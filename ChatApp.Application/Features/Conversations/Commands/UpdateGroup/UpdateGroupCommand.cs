using System;
using MediatR;

namespace ChatApp.Application.Features.Conversations.Commands.UpdateGroup;

public record UpdateGroupCommand(
    Guid ConversationId,
    Guid RequesterId,
    string? Title,
    string? AvatarUrl
) : IRequest;
