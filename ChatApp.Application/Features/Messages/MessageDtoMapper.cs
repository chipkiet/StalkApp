using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatApp.Application.DTOs.Messages;
using ChatApp.Application.Interfaces.Repositories;
using ChatApp.Domain.Entities;

namespace ChatApp.Application.Features.Messages;

public static class MessageDtoMapper
{
    public static async Task<MessageDto> ToDtoAsync(
        Message message,
        IGenericRepository<Attachment> attachmentRepository,
        IGenericRepository<MessageReaction>? reactionRepository = null,
        IGenericRepository<Message>? messageRepository = null)
    {
        var attachment = (await attachmentRepository.FindAsync(a => a.MessageId == message.Id)).FirstOrDefault();
        IReadOnlyList<ReactionDto> reactions = Array.Empty<ReactionDto>();
        if (reactionRepository is not null)
        {
            reactions = (await reactionRepository.FindAsync(r => r.MessageId == message.Id))
                .Select(r => new ReactionDto(r.UserId, r.Emotion))
                .ToList();
        }

        ReplyPreviewDto? replyTo = null;
        if (message.ReplyToMessageId.HasValue && messageRepository is not null)
        {
            var replied = await messageRepository.GetByIdAsync(message.ReplyToMessageId.Value);
            if (replied is not null)
            {
                var repliedAtt = (await attachmentRepository.FindAsync(a => a.MessageId == replied.Id)).FirstOrDefault();
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
            message.Id,
            message.ConversationId,
            message.SenderId,
            message.MessageType,
            message.IsDeleted ? null : message.Content,
            message.CreatedAt,
            message.IsDeleted ? null : attachment?.FileUrl,
            message.IsDeleted ? null : attachment?.FileName,
            message.IsPinned,
            message.IsDeleted,
            message.UpdatedAt,
            reactions,
            message.ReplyToMessageId,
            replyTo,
            message.ForwardedFromMessageId,
            message.ForwardedFromMessageId.HasValue
        );
    }
}
