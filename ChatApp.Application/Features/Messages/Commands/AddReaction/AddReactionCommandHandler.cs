using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ChatApp.Application.DTOs.Messages;
using ChatApp.Application.Interfaces.Repositories;
using ChatApp.Domain.Entities;
using MediatR;

namespace ChatApp.Application.Features.Messages.Commands.AddReaction;

public class AddReactionCommandHandler : IRequestHandler<AddReactionCommand, MessageDto>
{
    private readonly IGenericRepository<Message> _messageRepository;
    private readonly IGenericRepository<MessageReaction> _reactionRepository;
    private readonly IGenericRepository<Participant> _participantRepository;
    private readonly IGenericRepository<Attachment> _attachmentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddReactionCommandHandler(
        IGenericRepository<Message> messageRepository,
        IGenericRepository<MessageReaction> reactionRepository,
        IGenericRepository<Participant> participantRepository,
        IGenericRepository<Attachment> attachmentRepository,
        IUnitOfWork unitOfWork)
    {
        _messageRepository = messageRepository;
        _reactionRepository = reactionRepository;
        _participantRepository = participantRepository;
        _attachmentRepository = attachmentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<MessageDto> Handle(AddReactionCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Emotion))
            throw new InvalidOperationException("Emotion is required.");

        var emotion = request.Emotion.Trim();
        if (emotion.Length > 50)
            throw new InvalidOperationException("Emotion is too long.");

        var message = await _messageRepository.GetByIdAsync(request.MessageId)
            ?? throw new InvalidOperationException("Message not found.");

        if (message.IsDeleted)
            throw new InvalidOperationException("Cannot react to a deleted message.");

        var isParticipant = (await _participantRepository.FindAsync(
            p => p.ConversationId == message.ConversationId && p.UserId == request.UserId)).Any();

        if (!isParticipant)
            throw new UnauthorizedAccessException("Only conversation participants can react.");

        var existing = (await _reactionRepository.FindAsync(
            r => r.MessageId == request.MessageId && r.UserId == request.UserId)).FirstOrDefault();

        if (existing is null)
        {
            await _reactionRepository.AddAsync(new MessageReaction
            {
                MessageId = request.MessageId,
                UserId = request.UserId,
                Emotion = emotion
            });
        }
        else
        {
            existing.Emotion = emotion;
            _reactionRepository.Update(existing);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await MapToDtoAsync(message);
    }

    private async Task<MessageDto> MapToDtoAsync(Message message)
    {
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
