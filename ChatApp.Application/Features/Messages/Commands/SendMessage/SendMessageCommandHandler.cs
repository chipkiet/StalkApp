using System;
using System.Threading;
using System.Threading.Tasks;
using ChatApp.Application.DTOs.Messages;
using ChatApp.Application.Interfaces.Repositories;
using ChatApp.Domain.Entities;
using MediatR;

namespace ChatApp.Application.Features.Messages.Commands.SendMessage;

public class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, MessageDto>
{
    private readonly IGenericRepository<Message> _messageRepository;
    private readonly IGenericRepository<Attachment> _attachmentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SendMessageCommandHandler(
        IGenericRepository<Message> messageRepository, 
        IGenericRepository<Attachment> attachmentRepository,
        IUnitOfWork unitOfWork)
    {
        _messageRepository = messageRepository;
        _attachmentRepository = attachmentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<MessageDto> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        var message = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = request.ConversationId,
            SenderId = request.SenderId,
            MessageType = request.MessageType,
            Content = request.Content,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false,
            IsPinned = false
        };

        await _messageRepository.AddAsync(message);

        if (!string.IsNullOrEmpty(request.AttachmentUrl))
        {
            var attachment = new Attachment
            {
                Id = Guid.NewGuid(),
                MessageId = message.Id,
                FileUrl = request.AttachmentUrl,
                FileName = request.AttachmentName ?? "file",
                ContentType = request.AttachmentContentType ?? "application/octet-stream",
                FileSize = request.AttachmentSize ?? 0
            };
            await _attachmentRepository.AddAsync(attachment);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new MessageDto(
            message.Id,
            message.ConversationId,
            message.SenderId,
            message.MessageType,
            message.Content,
            message.CreatedAt,
            request.AttachmentUrl,
            request.AttachmentName,
            message.IsPinned,
            message.IsDeleted,
            message.UpdatedAt,
            Reactions: Array.Empty<ReactionDto>()
        );
    }
}
