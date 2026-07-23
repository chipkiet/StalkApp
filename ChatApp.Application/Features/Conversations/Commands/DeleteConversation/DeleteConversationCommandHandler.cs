using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ChatApp.Application.Interfaces.Repositories;
using ChatApp.Domain.Entities;
using MediatR;

namespace ChatApp.Application.Features.Conversations.Commands.DeleteConversation;

public class DeleteConversationCommandHandler : IRequestHandler<DeleteConversationCommand>
{
    private readonly IGenericRepository<Participant> _participantRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteConversationCommandHandler(
        IGenericRepository<Participant> participantRepository,
        IUnitOfWork unitOfWork)
    {
        _participantRepository = participantRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteConversationCommand request, CancellationToken cancellationToken)
    {
        var participant = _participantRepository.GetQueryable()
            .FirstOrDefault(p => p.ConversationId == request.ConversationId && p.UserId == request.UserId);

        if (participant != null)
        {
            participant.HasDeleted = true;
            participant.ClearedAt = DateTime.UtcNow;
            
            _participantRepository.Update(participant);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
