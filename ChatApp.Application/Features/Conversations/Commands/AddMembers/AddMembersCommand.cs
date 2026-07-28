using System;
using System.Collections.Generic;
using MediatR;

namespace ChatApp.Application.Features.Conversations.Commands.AddMembers;

public record AddMembersCommand(
    Guid ConversationId,
    Guid RequesterId,
    List<Guid> UserIds
) : IRequest;
