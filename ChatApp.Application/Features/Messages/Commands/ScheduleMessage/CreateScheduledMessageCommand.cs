using MediatR;
using System;

namespace ChatApp.Application.Features.Messages.Commands.ScheduleMessage;

public record CreateScheduledMessageCommand(
    Guid ConversationId,
    Guid SenderId,
    string Content,
    DateTime ScheduledAt) : IRequest<Guid>;
