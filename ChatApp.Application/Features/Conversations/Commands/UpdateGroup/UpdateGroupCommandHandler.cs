using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ChatApp.Application.Interfaces.Repositories;
using ChatApp.Domain.Entities;
using MediatR;

namespace ChatApp.Application.Features.Conversations.Commands.UpdateGroup;

public class UpdateGroupCommandHandler : IRequestHandler<UpdateGroupCommand>
{
    private readonly IGenericRepository<Conversation> _conversationRepository;
    private readonly IGenericRepository<Participant> _participantRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateGroupCommandHandler(
        IGenericRepository<Conversation> conversationRepository,
        IGenericRepository<Participant> participantRepository,
        IUnitOfWork unitOfWork)
    {
        _conversationRepository = conversationRepository;
        _participantRepository = participantRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateGroupCommand request, CancellationToken cancellationToken)
    {
        // Kiểm tra requester có là thành viên của nhóm không
        var isParticipant = (await _participantRepository.FindAsync(
            p => p.ConversationId == request.ConversationId && p.UserId == request.RequesterId))
            .Any();

        if (!isParticipant)
            throw new UnauthorizedAccessException("Bạn không có quyền chỉnh sửa nhóm này.");

        var conversation = await _conversationRepository.GetByIdAsync(request.ConversationId)
            ?? throw new InvalidOperationException("Nhóm không tồn tại.");

        if (request.Title != null)
            conversation.Title = request.Title;

        if (request.AvatarUrl != null)
            conversation.AvatarUrl = request.AvatarUrl;

        _conversationRepository.Update(conversation);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
