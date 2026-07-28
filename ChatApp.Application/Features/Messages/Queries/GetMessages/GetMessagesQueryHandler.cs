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
    private readonly IGenericRepository<Participant> _participantRepository;

    public GetMessagesQueryHandler(
        IGenericRepository<Message> messageRepository,
        IGenericRepository<Attachment> attachmentRepository,
        IGenericRepository<MessageReaction> reactionRepository,
        IGenericRepository<Participant> participantRepository)
    {
        _messageRepository = messageRepository;
        _attachmentRepository = attachmentRepository;
        _reactionRepository = reactionRepository;
        _participantRepository = participantRepository;
    }

    public async Task<List<MessageDto>> Handle(GetMessagesQuery request, CancellationToken cancellationToken)
    {
        var participants = await _participantRepository.FindAsync(p => p.ConversationId == request.ConversationId && p.UserId == request.UserId);
        var participant = participants.FirstOrDefault();
        
        if (participant == null)
            throw new UnauthorizedAccessException("Bạn không có quyền truy cập vào cuộc trò chuyện này.");
            
        var messagesQuery = _messageRepository.GetQueryable()
            .Where(m => m.ConversationId == request.ConversationId);

        if (participant != null && participant.ClearedAt.HasValue)
        {
            messagesQuery = messagesQuery.Where(m => m.CreatedAt >= participant.ClearedAt.Value);
        }

        var messageList = messagesQuery.OrderByDescending(m => m.CreatedAt).Take(request.Count).ToList();

        var messageIds = messageList.Select(m => m.Id).ToList();
        var replyIds = messageList
            .Where(m => m.ReplyToMessageId.HasValue)
            .Select(m => m.ReplyToMessageId!.Value)
            .Distinct()
            .ToList();

        var attachments = await _attachmentRepository.FindAsync(a => messageIds.Contains(a.MessageId));
        var reactions = await _reactionRepository.FindAsync(r => messageIds.Contains(r.MessageId));

        var replyMessages = replyIds.Count == 0
            ? new List<Message>()
            : (await _messageRepository.FindAsync(m => replyIds.Contains(m.Id))).ToList();
        var replyAttachments = replyIds.Count == 0
            ? new List<Attachment>()
            : (await _attachmentRepository.FindAsync(a => replyIds.Contains(a.MessageId))).ToList();

        var result = messageList.Select(m =>
        {
            var att = attachments.FirstOrDefault(a => a.MessageId == m.Id);
            var messageReactions = reactions
                .Where(r => r.MessageId == m.Id)
                .Select(r => new ReactionDto(r.UserId, r.Emotion))
                .ToList();

            ReplyPreviewDto? replyTo = null;
            if (m.ReplyToMessageId.HasValue)
            {
                var replied = replyMessages.FirstOrDefault(r => r.Id == m.ReplyToMessageId.Value);
                if (replied is not null)
                {
                    var repliedAtt = replyAttachments.FirstOrDefault(a => a.MessageId == replied.Id);
                    replyTo = new ReplyPreviewDto(
                        replied.Id,
                        replied.SenderId,
                        replied.IsDeleted ? null : replied.Content,
                        replied.IsDeleted,
                        replied.IsDeleted ? null : repliedAtt?.FileName
                    );
                }
            }

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
                messageReactions,
                m.ReplyToMessageId,
                replyTo,
                m.ForwardedFromMessageId,
                m.ForwardedFromMessageId.HasValue
            );
        }).ToList();

        result.Reverse();
        return result;
    }
}
