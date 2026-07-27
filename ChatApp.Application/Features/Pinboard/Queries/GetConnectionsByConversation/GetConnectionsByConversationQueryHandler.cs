using ChatApp.Application.Interfaces.Repositories;
using ChatApp.Shared.DTOs.Pinboard;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ChatApp.Application.Features.Pinboard.Queries.GetConnectionsByConversation;

public class GetConnectionsByConversationQueryHandler : IRequestHandler<GetConnectionsByConversationQuery, List<PinboardConnectionDto>>
{
    private readonly IGenericRepository<Domain.Entities.PinboardConnection> _repository;

    public GetConnectionsByConversationQueryHandler(IGenericRepository<Domain.Entities.PinboardConnection> repository)
    {
        _repository = repository;
    }

    public async Task<List<PinboardConnectionDto>> Handle(GetConnectionsByConversationQuery request, CancellationToken cancellationToken)
    {
        var items = await _repository.FindAsync(c => c.ConversationId == request.ConversationId);
        
        return items.Select(c => new PinboardConnectionDto
        {
            Id = c.Id,
            ConversationId = c.ConversationId,
            SourceItemId = c.SourceItemId,
            TargetItemId = c.TargetItemId,
            Label = c.Label
        }).ToList();
    }
}
