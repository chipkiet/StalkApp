using MediatR;
using System;

namespace ChatApp.Application.Features.Pinboard.Commands.DeletePinboardConnection;

public record DeletePinboardConnectionCommand(Guid Id) : IRequest<bool>;
