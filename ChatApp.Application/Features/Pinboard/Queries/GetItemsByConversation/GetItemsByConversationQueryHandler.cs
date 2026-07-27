using ChatApp.Application.Interfaces.Repositories;
using ChatApp.Shared.DTOs.Pinboard;
using MediatR;

namespace ChatApp.Application.Features.Pinboard.Queries.GetItemsByConversation;

public class GetItemsByConversationQueryHandler : IRequestHandler<GetItemsByConversationQuery, List<PinboardItemDto>>
{
    private readonly IGenericRepository<Domain.Entities.PinboardItem> _repository;

    public GetItemsByConversationQueryHandler(IGenericRepository<Domain.Entities.PinboardItem> repository)
    {
        _repository = repository;
    }

    public async Task<List<PinboardItemDto>> Handle(GetItemsByConversationQuery request, CancellationToken cancellationToken)
    {
        var items = await _repository.FindAsync(i => i.ConversationId == request.ConversationId);

        var sortedItems = items.OrderBy(i => i.CreatedAt).ToList();

        return sortedItems.Select(i => new PinboardItemDto
        {
            Id = i.Id,
            ConversationId = i.ConversationId,
            LinkedMessageId = i.LinkedMessageId,
            Type = i.Type,
            Content = i.Content,
            PositionX = i.PositionX,
            PositionY = i.PositionY,
            ZIndex = i.ZIndex,
            AssignedToUserId = i.AssignedToUserId,
            IsCompleted = i.IsCompleted,
            CompletedAt = i.CompletedAt,
            Deadline = i.Deadline,
            CreatedAt = i.CreatedAt
        }).ToList();
    }
}
