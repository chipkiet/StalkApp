using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ChatApp.Application.Interfaces.Repositories;
using ChatApp.Domain.Entities;
using ChatApp.Domain.Enums;
using MediatR;

namespace ChatApp.Application.Features.Conversations.Commands.RemoveMember;

public class RemoveMemberCommandHandler : IRequestHandler<RemoveMemberCommand>
{
    private readonly IGenericRepository<Participant> _participantRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveMemberCommandHandler(
        IGenericRepository<Participant> participantRepository,
        IUnitOfWork unitOfWork)
    {
        _participantRepository = participantRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(RemoveMemberCommand request, CancellationToken cancellationToken)
    {
        // Kiểm tra requester có phải Admin không
        var requesterParticipant = (await _participantRepository.FindAsync(
            p => p.ConversationId == request.ConversationId && p.UserId == request.RequesterId))
            .FirstOrDefault();

        if (requesterParticipant == null || requesterParticipant.Role != ParticipantRole.Admin)
            throw new UnauthorizedAccessException("Chỉ trưởng nhóm mới có thể xóa thành viên.");

        if (request.TargetUserId == request.RequesterId)
            throw new InvalidOperationException("Trưởng nhóm không thể tự xóa mình. Hãy giải tán nhóm nếu muốn rời.");

        var targetParticipant = (await _participantRepository.FindAsync(
            p => p.ConversationId == request.ConversationId && p.UserId == request.TargetUserId))
            .FirstOrDefault();

        if (targetParticipant == null)
            throw new InvalidOperationException("Thành viên không tồn tại trong nhóm.");

        _participantRepository.Remove(targetParticipant);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
