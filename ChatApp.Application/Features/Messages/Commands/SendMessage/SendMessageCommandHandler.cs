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
        if (request.ReplyToMessageId.HasValue)
        {
            var replyTarget = await _messageRepository.GetByIdAsync(request.ReplyToMessageId.Value)
                ?? throw new InvalidOperationException("Reply target message not found.");

            if (replyTarget.ConversationId != request.ConversationId)
                throw new InvalidOperationException("Can only reply to a message in the same conversation.");
        }

        var message = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = request.ConversationId,
            SenderId = request.SenderId,
            MessageType = request.MessageType,
            Content = request.Content,
            ReplyToMessageId = request.ReplyToMessageId,
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

        return await MessageDtoMapper.ToDtoAsync(message, _attachmentRepository, messageRepository: _messageRepository);
    }
}
