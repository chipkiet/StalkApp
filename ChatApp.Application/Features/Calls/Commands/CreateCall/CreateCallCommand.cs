using System;
using ChatApp.Domain.Enums;
using MediatR;

namespace ChatApp.Application.Features.Calls.Commands.CreateCall;

public record CreateCallCommand(
    Guid Id,
    Guid ConversationId,
    Guid CallerId,
    CallType Type
) : IRequest<Guid>;
