using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ChatApp.Application.Interfaces.Repositories;
using ChatApp.Domain.Entities;
using ChatApp.Domain.Enums;
using MediatR;

namespace ChatApp.Application.Features.Conversations.Commands.DisbandGroup;

public class DisbandGroupCommandHandler : IRequestHandler<DisbandGroupCommand>
{
    private readonly IGenericRepository<Conversation> _conversationRepository;
    private readonly IGenericRepository<Participant> _participantRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DisbandGroupCommandHandler(
        IGenericRepository<Conversation> conversationRepository,
        IGenericRepository<Participant> participantRepository,
        IUnitOfWork unitOfWork)
    {
        _conversationRepository = conversationRepository;
        _participantRepository = participantRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DisbandGroupCommand request, CancellationToken cancellationToken)
    {
        // Kiểm tra requester có phải Admin không
        var requesterParticipant = (await _participantRepository.FindAsync(
            p => p.ConversationId == request.ConversationId && p.UserId == request.RequesterId))
            .FirstOrDefault();

        if (requesterParticipant == null || requesterParticipant.Role != ParticipantRole.Admin)
            throw new UnauthorizedAccessException("Chỉ trưởng nhóm mới có thể giải tán nhóm.");

        var conversation = await _conversationRepository.GetByIdAsync(request.ConversationId)
            ?? throw new InvalidOperationException("Nhóm không tồn tại.");

        if (conversation.Type != ConversationType.Group)
            throw new InvalidOperationException("Chỉ có thể giải tán nhóm chat.");

        // Xóa tất cả participants
        var allParticipants = await _participantRepository.FindAsync(
            p => p.ConversationId == request.ConversationId);

        foreach (var p in allParticipants)
            _participantRepository.Remove(p);

        // Xóa conversation
        _conversationRepository.Remove(conversation);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
