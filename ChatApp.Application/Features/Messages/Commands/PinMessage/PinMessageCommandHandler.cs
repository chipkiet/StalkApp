using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ChatApp.Application.DTOs.Messages;
using ChatApp.Application.Interfaces.Repositories;
using ChatApp.Domain.Entities;
using MediatR;

namespace ChatApp.Application.Features.Messages.Commands.PinMessage;

public class PinMessageCommandHandler : IRequestHandler<PinMessageCommand, MessageDto>
{
    private readonly IGenericRepository<Message> _messageRepository;
    private readonly IGenericRepository<Participant> _participantRepository;
    private readonly IGenericRepository<Attachment> _attachmentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PinMessageCommandHandler(
        IGenericRepository<Message> messageRepository,
        IGenericRepository<Participant> participantRepository,
        IGenericRepository<Attachment> attachmentRepository,
        IUnitOfWork unitOfWork)
    {
        _messageRepository = messageRepository;
        _participantRepository = participantRepository;
        _attachmentRepository = attachmentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<MessageDto> Handle(PinMessageCommand request, CancellationToken cancellationToken)
    {
        var message = await _messageRepository.GetByIdAsync(request.MessageId)
            ?? throw new InvalidOperationException("Message not found.");

        if (message.IsDeleted)
            throw new InvalidOperationException("Cannot pin a deleted message.");

        var isParticipant = (await _participantRepository.FindAsync(
            p => p.ConversationId == message.ConversationId && p.UserId == request.UserId)).Any();

        if (!isParticipant)
            throw new UnauthorizedAccessException("Only conversation participants can pin messages.");

        message.IsPinned = request.IsPinned;
        message.UpdatedAt = DateTime.UtcNow;

        _messageRepository.Update(message);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var attachment = (await _attachmentRepository.FindAsync(a => a.MessageId == message.Id)).FirstOrDefault();

        return new MessageDto(
            message.Id,
            message.ConversationId,
            message.SenderId,
            message.MessageType,
            message.Content,
            message.CreatedAt,
            attachment?.FileUrl,
            attachment?.FileName,
            message.IsPinned,
            message.IsDeleted,
            message.UpdatedAt
        );
    }
}
