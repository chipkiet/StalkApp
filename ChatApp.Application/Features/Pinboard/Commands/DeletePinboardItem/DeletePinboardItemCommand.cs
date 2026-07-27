using MediatR;

namespace ChatApp.Application.Features.Pinboard.Commands.DeletePinboardItem;

public record DeletePinboardItemCommand(Guid Id) : IRequest<bool>;
