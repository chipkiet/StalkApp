using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ChatApp.Application.Interfaces.Repositories;
using ChatApp.Domain.Entities;
using ChatApp.Domain.Enums;
using MediatR;

namespace ChatApp.Application.Features.Conversations.Commands.CreateConversation;

public class CreateConversationCommandHandler : IRequestHandler<CreateConversationCommand, Guid>
{
    private readonly IGenericRepository<Conversation> _conversationRepository;
    private readonly IGenericRepository<Participant> _participantRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateConversationCommandHandler(
        IGenericRepository<Conversation> conversationRepository,
        IGenericRepository<Participant> participantRepository,
        IUnitOfWork unitOfWork)
    {
        _conversationRepository = conversationRepository;
        _participantRepository = participantRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateConversationCommand request, CancellationToken cancellationToken)
    {
        // 1. Tạo Conversation
        var conversation = new Conversation
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Type = request.Type,
            CreatedAt = DateTime.UtcNow
        };

        await _conversationRepository.AddAsync(conversation);

        // 2. Thêm Creator vào danh sách tham gia
        var participantIds = request.ParticipantIds.ToList();
        if (!participantIds.Contains(request.CreatorId))
        {
            participantIds.Add(request.CreatorId);
        }

        // Kiểm tra xem cuộc trò chuyện Direct giữa 2 người này đã tồn tại chưa
        if (request.Type == ConversationType.Direct && participantIds.Count == 2)
        {
            var u1 = participantIds[0];
            var u2 = participantIds[1];
            var existingConvId = _conversationRepository.GetQueryable()
                .Where(c => c.Type == ConversationType.Direct)
                .Where(c => c.Participants.Any(p => p.UserId == u1) && c.Participants.Any(p => p.UserId == u2))
                .Select(c => c.Id)
                .FirstOrDefault();

            if (existingConvId != Guid.Empty)
            {
                var existingParticipant = _participantRepository.GetQueryable()
                    .FirstOrDefault(p => p.ConversationId == existingConvId && p.UserId == request.CreatorId);
                
                if (existingParticipant != null && existingParticipant.HasDeleted)
                {
                    existingParticipant.HasDeleted = false;
                    _participantRepository.Update(existingParticipant);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }

                return existingConvId;
            }
        }

        // 3. Tạo Participants
        foreach (var userId in participantIds)
        {
            var participant = new Participant
            {
                ConversationId = conversation.Id,
                UserId = userId,
                Role = userId == request.CreatorId ? ParticipantRole.Admin : ParticipantRole.Member,
                JoinedAt = DateTime.UtcNow
            };
            await _participantRepository.AddAsync(participant);
        }

        // 4. Lưu Atomically
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return conversation.Id;
    }
}
