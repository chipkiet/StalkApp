using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ChatApp.Application.Interfaces.Repositories;
using ChatApp.Domain.Entities;
using MediatR;

namespace ChatApp.Application.Features.Conversations.Commands.ToggleMute;

public class ToggleMuteCommandHandler : IRequestHandler<ToggleMuteCommand>
{
    private readonly IGenericRepository<Participant> _participantRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ToggleMuteCommandHandler(IGenericRepository<Participant> participantRepository, IUnitOfWork unitOfWork)
    {
        _participantRepository = participantRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ToggleMuteCommand request, CancellationToken cancellationToken)
    {
        var participants = await _participantRepository.FindAsync(p => p.ConversationId == request.ConversationId && p.UserId == request.UserId);
        var participant = participants.FirstOrDefault();

        if (participant != null)
        {
            participant.IsMuted = request.IsMuted;
            _participantRepository.Update(participant);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
