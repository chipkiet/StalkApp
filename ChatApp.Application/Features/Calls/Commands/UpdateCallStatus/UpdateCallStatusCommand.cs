using System;
using ChatApp.Domain.Enums;
using MediatR;

namespace ChatApp.Application.Features.Calls.Commands.UpdateCallStatus;

public record UpdateCallStatusCommand(
    Guid CallId,
    CallStatus Status
) : IRequest<bool>;
