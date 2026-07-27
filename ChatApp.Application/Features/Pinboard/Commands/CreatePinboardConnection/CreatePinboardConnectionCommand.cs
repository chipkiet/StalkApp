using ChatApp.Shared.DTOs.Pinboard;
using MediatR;
using System;

namespace ChatApp.Application.Features.Pinboard.Commands.CreatePinboardConnection;

public record CreatePinboardConnectionCommand(Guid ConversationId, Guid SourceItemId, Guid TargetItemId, string? Label) : IRequest<PinboardConnectionDto>;
