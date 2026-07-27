using System;
using System.Threading;
using System.Threading.Tasks;
using ChatApp.Application.Interfaces.Repositories;
using ChatApp.Domain.Entities;
using ChatApp.Shared.DTOs.Pinboard;
using MediatR;

namespace ChatApp.Application.Features.Pinboard.Commands.MovePinboardItem;

public record MovePinboardItemCommand(Guid Id, double PositionX, double PositionY, int ZIndex) : IRequest<PinboardItemDto?>;

public class MovePinboardItemCommandHandler : IRequestHandler<MovePinboardItemCommand, PinboardItemDto?>
{
    private readonly IGenericRepository<PinboardItem> _pinboardRepository;
    private readonly IUnitOfWork _unitOfWork;

    public MovePinboardItemCommandHandler(IGenericRepository<PinboardItem> pinboardRepository, IUnitOfWork unitOfWork)
    {
        _pinboardRepository = pinboardRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<PinboardItemDto?> Handle(MovePinboardItemCommand request, CancellationToken cancellationToken)
    {
        var item = await _pinboardRepository.GetByIdAsync(request.Id);
        if (item == null) return null;

        item.PositionX = request.PositionX;
        item.PositionY = request.PositionY;
        item.ZIndex = request.ZIndex;
        item.UpdatedAt = DateTime.UtcNow;

        _pinboardRepository.Update(item);
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
