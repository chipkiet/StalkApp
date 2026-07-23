using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ChatApp.Application.DTOs.Messages;
using ChatApp.Application.Interfaces.Repositories;
using ChatApp.Domain.Entities;
using MediatR;

namespace ChatApp.Application.Features.Messages.Commands.RemoveReaction;

public class RemoveReactionCommandHandler : IRequestHandler<RemoveReactionCommand, MessageDto>
{
    private readonly IGenericRepository<Message> _messageRepository;
    private readonly IGenericRepository<MessageReaction> _reactionRepository;
    private readonly IGenericRepository<Attachment> _attachmentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveReactionCommandHandler(
        IGenericRepository<Message> messageRepository,
        IGenericRepository<MessageReaction> reactionRepository,
        IGenericRepository<Attachment> attachmentRepository,
        IUnitOfWork unitOfWork)
    {
        _messageRepository = messageRepository;
        _reactionRepository = reactionRepository;
        _attachmentRepository = attachmentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<MessageDto> Handle(RemoveReactionCommand request, CancellationToken cancellationToken)
    {
        var message = await _messageRepository.GetByIdAsync(request.MessageId)
            ?? throw new InvalidOperationException("Message not found.");

        var existing = (await _reactionRepository.FindAsync(
            r => r.MessageId == request.MessageId && r.UserId == request.UserId)).FirstOrDefault()
            ?? throw new InvalidOperationException("Reaction not found.");

        _reactionRepository.Remove(existing);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var attachment = (await _attachmentRepository.FindAsync(a => a.MessageId == message.Id)).FirstOrDefault();
        var reactions = (await _reactionRepository.FindAsync(r => r.MessageId == message.Id))
            .Select(r => new ReactionDto(r.UserId, r.Emotion))
            .ToList();

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
            message.UpdatedAt,
            reactions
        );
    }
}
