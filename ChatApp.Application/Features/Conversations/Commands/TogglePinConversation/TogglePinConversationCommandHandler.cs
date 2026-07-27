using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ChatApp.Application.Interfaces.Repositories;
using ChatApp.Domain.Entities;
using MediatR;

namespace ChatApp.Application.Features.Conversations.Commands.TogglePinConversation;

public class TogglePinConversationCommandHandler : IRequestHandler<TogglePinConversationCommand, bool>
{
    private readonly IGenericRepository<Participant> _participantRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TogglePinConversationCommandHandler(
        IGenericRepository<Participant> participantRepository,
        IUnitOfWork unitOfWork)
    {
        _participantRepository = participantRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(TogglePinConversationCommand request, CancellationToken cancellationToken)
    {
        var participant = (await _participantRepository.FindAsync(
            p => p.ConversationId == request.ConversationId && p.UserId == request.UserId)).FirstOrDefault();

        if (participant == null)
            throw new UnauthorizedAccessException("You are not a participant of this conversation.");

        participant.IsPinned = request.IsPinned;
        _participantRepository.Update(participant);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return request.IsPinned;
    }
}
