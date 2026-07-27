using System;
using System.Threading;
using System.Threading.Tasks;
using ChatApp.Domain.Entities;
using ChatApp.Application.Interfaces.Repositories;
using ChatApp.Shared.DTOs.Pinboard;
using ChatApp.Shared.Enums;
using MediatR;

namespace ChatApp.Application.Features.Pinboard.Commands.CreatePinboardItem;

public record CreatePinboardItemCommand(
    Guid ConversationId,
    PinboardItemType Type,
    string? Content,
    double PositionX,
    double PositionY,
    Guid? LinkedMessageId,
    Guid? AssignedToUserId,
    DateTime? Deadline) : IRequest<PinboardItemDto>;

public class CreatePinboardItemCommandHandler : IRequestHandler<CreatePinboardItemCommand, PinboardItemDto>
{
    private readonly IGenericRepository<PinboardItem> _pinboardRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreatePinboardItemCommandHandler(IGenericRepository<PinboardItem> pinboardRepository, IUnitOfWork unitOfWork)
    {
        _pinboardRepository = pinboardRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<PinboardItemDto> Handle(CreatePinboardItemCommand request, CancellationToken cancellationToken)
    {
        var item = new PinboardItem
        {
            Id = Guid.NewGuid(),
            ConversationId = request.ConversationId,
            Type = request.Type,
            Content = request.Content,
            PositionX = request.PositionX,
            PositionY = request.PositionY,
            LinkedMessageId = request.LinkedMessageId,
            AssignedToUserId = request.AssignedToUserId,
            Deadline = request.Deadline,
            ZIndex = 1
        };

        await _pinboardRepository.AddAsync(item);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new PinboardItemDto
        {
            Id = item.Id,
            ConversationId = item.ConversationId,
            LinkedMessageId = item.LinkedMessageId,
            Type = item.Type,
            Content = item.Content,
            PositionX = item.PositionX,
            PositionY = item.PositionY,
            ZIndex = item.ZIndex,
            AssignedToUserId = item.AssignedToUserId,
            IsCompleted = item.IsCompleted,
            CompletedAt = item.CompletedAt,
            Deadline = item.Deadline,
            CreatedAt = item.CreatedAt
        };
    }
}
