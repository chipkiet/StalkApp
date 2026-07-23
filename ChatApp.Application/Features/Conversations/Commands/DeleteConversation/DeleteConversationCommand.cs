using System;
using MediatR;

namespace ChatApp.Application.Features.Conversations.Commands.DeleteConversation;

public record DeleteConversationCommand(Guid ConversationId, Guid UserId) : IRequest;
