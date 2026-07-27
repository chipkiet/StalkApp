using System;
using MediatR;

namespace ChatApp.Application.Features.Conversations.Commands.TogglePinConversation;

public record TogglePinConversationCommand(Guid ConversationId, Guid UserId, bool IsPinned) : IRequest<bool>;
