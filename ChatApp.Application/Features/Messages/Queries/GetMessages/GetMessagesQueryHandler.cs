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

    public GetMessagesQueryHandler(
        IGenericRepository<Message> messageRepository,
        IGenericRepository<Attachment> attachmentRepository)
    {
        _messageRepository = messageRepository;
        _attachmentRepository = attachmentRepository;
    }

    public async Task<List<MessageDto>> Handle(GetMessagesQuery request, CancellationToken cancellationToken)
    {
        var messages = await _messageRepository.FindAsync(m => m.ConversationId == request.ConversationId);
        var messageList = messages.OrderByDescending(m => m.CreatedAt).Take(request.Count).ToList();

        var messageIds = messageList.Select(m => m.Id).ToList();
        var attachments = await _attachmentRepository.FindAsync(a => messageIds.Contains(a.MessageId));

        var result = messageList.Select(m => {
            var att = attachments.FirstOrDefault(a => a.MessageId == m.Id);
            return new MessageDto(
                m.Id,
                m.ConversationId,
                m.SenderId,
                m.MessageType,
                m.Content,
                m.CreatedAt,
                att?.FileUrl,
                att?.FileName
            );
        }).ToList();

        // Lật ngược lại để trả về theo thứ tự cũ -> mới (hiển thị chat từ trên xuống)
        result.Reverse();

        return result;
    }
}
