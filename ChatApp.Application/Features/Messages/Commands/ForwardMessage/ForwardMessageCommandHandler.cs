using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ChatApp.Application.DTOs.Messages;
using ChatApp.Application.Interfaces.Repositories;
using ChatApp.Domain.Entities;
using MediatR;

namespace ChatApp.Application.Features.Messages.Commands.ForwardMessage;

public class ForwardMessageCommandHandler : IRequestHandler<ForwardMessageCommand, MessageDto>
{
    private readonly IGenericRepository<Message> _messageRepository;
    private readonly IGenericRepository<Attachment> _attachmentRepository;
    private readonly IGenericRepository<Participant> _participantRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ForwardMessageCommandHandler(
        IGenericRepository<Message> messageRepository,
        IGenericRepository<Attachment> attachmentRepository,
        IGenericRepository<Participant> participantRepository,
        IUnitOfWork unitOfWork)
    {
        _messageRepository = messageRepository;
        _attachmentRepository = attachmentRepository;
        _participantRepository = participantRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<MessageDto> Handle(ForwardMessageCommand request, CancellationToken cancellationToken)
    {
        var source = await _messageRepository.GetByIdAsync(request.MessageId)
            ?? throw new InvalidOperationException("Source message not found.");

        if (source.IsDeleted)
            throw new InvalidOperationException("Cannot forward a deleted message.");

        var isInSource = (await _participantRepository.FindAsync(
            p => p.ConversationId == source.ConversationId && p.UserId == request.SenderId)).Any();
        if (!isInSource)
            throw new UnauthorizedAccessException("You are not a participant of the source conversation.");

        var isInTarget = (await _participantRepository.FindAsync(
            p => p.ConversationId == request.TargetConversationId && p.UserId == request.SenderId)).Any();
        if (!isInTarget)
            throw new UnauthorizedAccessException("You are not a participant of the target conversation.");

        var forwarded = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = request.TargetConversationId,
            SenderId = request.SenderId,
            MessageType = source.MessageType,
            Content = source.Content,
            ForwardedFromMessageId = source.Id,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false,
            IsPinned = false
        };

        await _messageRepository.AddAsync(forwarded);

        var sourceAttachment = (await _attachmentRepository.FindAsync(a => a.MessageId == source.Id)).FirstOrDefault();
        if (sourceAttachment is not null)
        {
            await _attachmentRepository.AddAsync(new Attachment
            {
                Id = Guid.NewGuid(),
                MessageId = forwarded.Id,
                FileUrl = sourceAttachment.FileUrl,
                FileName = sourceAttachment.FileName,
                ContentType = sourceAttachment.ContentType,
                FileSize = sourceAttachment.FileSize
            });
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await MessageDtoMapper.ToDtoAsync(forwarded, _attachmentRepository, messageRepository: _messageRepository);
    }
}
