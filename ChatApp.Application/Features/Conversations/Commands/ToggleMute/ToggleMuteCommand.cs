using MediatR;
using System;

namespace ChatApp.Application.Features.Conversations.Commands.ToggleMute;

public record ToggleMuteCommand(Guid ConversationId, Guid UserId, bool IsMuted) : IRequest;
