using ChatApp.Shared.DTOs.Pinboard;
using MediatR;
using System;

namespace ChatApp.Application.Features.Pinboard.Commands.UpdatePinboardItemContent;

public record UpdatePinboardItemContentCommand(Guid Id, string Content, string? Color = null) : IRequest<bool>;
