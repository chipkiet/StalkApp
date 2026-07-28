using System;
using MediatR;

namespace ChatApp.Application.Features.Conversations.Commands.RemoveMember;

public record RemoveMemberCommand(
    Guid ConversationId,
    Guid RequesterId,
    Guid TargetUserId
) : IRequest;
