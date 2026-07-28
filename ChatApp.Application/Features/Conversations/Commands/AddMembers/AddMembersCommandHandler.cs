using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ChatApp.Application.Interfaces.Repositories;
using ChatApp.Domain.Entities;
using ChatApp.Domain.Enums;
using MediatR;

namespace ChatApp.Application.Features.Conversations.Commands.AddMembers;

public class AddMembersCommandHandler : IRequestHandler<AddMembersCommand>
{
    private readonly IGenericRepository<Participant> _participantRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddMembersCommandHandler(
        IGenericRepository<Participant> participantRepository,
        IUnitOfWork unitOfWork)
    {
        _participantRepository = participantRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(AddMembersCommand request, CancellationToken cancellationToken)
    {
        // Kiểm tra requester có là thành viên không
        var isParticipant = (await _participantRepository.FindAsync(
            p => p.ConversationId == request.ConversationId && p.UserId == request.RequesterId))
            .Any();

        if (!isParticipant)
            throw new UnauthorizedAccessException("Bạn không có quyền thêm thành viên vào nhóm này.");

        foreach (var userId in request.UserIds)
        {
            // Chỉ thêm nếu chưa là thành viên
            var alreadyExists = (await _participantRepository.FindAsync(
                p => p.ConversationId == request.ConversationId && p.UserId == userId))
                .Any();

            if (!alreadyExists)
            {
                var participant = new Participant
                {
                    ConversationId = request.ConversationId,
                    UserId = userId,
                    Role = ParticipantRole.Member,
                    JoinedAt = DateTime.UtcNow
                };
                await _participantRepository.AddAsync(participant);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
