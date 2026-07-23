using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ChatApp.Application.DTOs.Messages;
using ChatApp.Application.Interfaces.Repositories;
using ChatApp.Domain.Entities;
using MediatR;

namespace ChatApp.Application.Features.Messages.Queries.GetMessages;

public class GetMessagesQueryHandler : IRequestHandler<GetMessagesQuery, List<MessageDto>>
{
    private readonly IGenericRepository<Message> _messageRepository;
    private readonly IGenericRepository<Attachment> _attachmentRepository;
    private readonly IGenericRepository<MessageReaction> _reactionRepository;

    public GetMessagesQueryHandler(
        IGenericRepository<Message> messageRepository,
        IGenericRepository<Attachment> attachmentRepository,
        IGenericRepository<MessageReaction> reactionRepository)
    {
        _messageRepository = messageRepository;
        _attachmentRepository = attachmentRepository;
        _reactionRepository = reactionRepository;
    }

    public async Task<List<MessageDto>> Handle(GetMessagesQuery request, CancellationToken cancellationToken)
    {
        var messages = await _messageRepository.FindAsync(m => m.ConversationId == request.ConversationId);
        var messageList = messages.OrderByDescending(m => m.CreatedAt).Take(request.Count).ToList();

        var messageIds = messageList.Select(m => m.Id).ToList();
        var attachments = await _attachmentRepository.FindAsync(a => messageIds.Contains(a.MessageId));
        var reactions = await _reactionRepository.FindAsync(r => messageIds.Contains(r.MessageId));

        var result = messageList.Select(m =>
        {
            var att = attachments.FirstOrDefault(a => a.MessageId == m.Id);
            var messageReactions = reactions
                .Where(r => r.MessageId == m.Id)
                .Select(r => new ReactionDto(r.UserId, r.Emotion))
                .ToList();

            return new MessageDto(
                m.Id,
                m.ConversationId,
                m.SenderId,
                m.MessageType,
                m.IsDeleted ? null : m.Content,
                m.CreatedAt,
                m.IsDeleted ? null : att?.FileUrl,
                m.IsDeleted ? null : att?.FileName,
                m.IsPinned,
                m.IsDeleted,
                m.UpdatedAt,
                messageReactions
            );
        }).ToList();

        result.Reverse();
        return result;
    }
}
