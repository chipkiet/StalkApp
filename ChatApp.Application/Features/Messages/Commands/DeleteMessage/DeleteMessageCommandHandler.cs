using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ChatApp.Application.DTOs.Messages;
using ChatApp.Application.Interfaces.Repositories;
using ChatApp.Domain.Entities;
using ChatApp.Domain.Enums;
using MediatR;

namespace ChatApp.Application.Features.Messages.Commands.DeleteMessage;

public class DeleteMessageCommandHandler : IRequestHandler<DeleteMessageCommand, MessageDto>
{
    private readonly IGenericRepository<Message> _messageRepository;
    private readonly IGenericRepository<Participant> _participantRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteMessageCommandHandler(
        IGenericRepository<Message> messageRepository,
        IGenericRepository<Participant> participantRepository,
        IUnitOfWork unitOfWork)
    {
        _messageRepository = messageRepository;
        _participantRepository = participantRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<MessageDto> Handle(DeleteMessageCommand request, CancellationToken cancellationToken)
    {
        var message = await _messageRepository.GetByIdAsync(request.MessageId)
            ?? throw new InvalidOperationException("Message not found.");

        if (message.IsDeleted)
            throw new InvalidOperationException("Message is already deleted.");

        // Authorization: Tác giả có thể xoá tin của mình.
        // Admin của nhóm cũng có thể xoá tin bất kỳ (quyền kiểm duyệt).
        bool isAuthor = message.SenderId == request.UserId;

        if (!isAuthor)
        {
            // Kiểm tra xem người dùng có phải Admin của cuộc trò chuyện đó không
            var callerParticipant = (await _participantRepository.FindAsync(
                p => p.ConversationId == message.ConversationId && p.UserId == request.UserId))
                .FirstOrDefault();

            bool isAdmin = callerParticipant?.Role == ParticipantRole.Admin;

            if (!isAdmin)
                throw new UnauthorizedAccessException(
                    "Only the message author or a conversation admin can delete this message.");
        }

        message.IsDeleted = true;
        message.Content = null;
        message.UpdatedAt = DateTime.UtcNow;

        _messageRepository.Update(message);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new MessageDto(
            message.Id,
            message.ConversationId,
            message.SenderId,
            message.MessageType,
            null,
            message.CreatedAt,
            null,
            null,
            message.IsPinned,
            true,
            message.UpdatedAt
        );
    }
}
