using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ChatApp.Application.DTOs.Conversations;
using ChatApp.Application.Interfaces.Repositories;
using ChatApp.Domain.Entities;
using MediatR;

namespace ChatApp.Application.Features.Conversations.Queries.GetInbox;

public class GetInboxQueryHandler : IRequestHandler<GetInboxQuery, List<InboxItemDto>>
{
    private readonly IGenericRepository<Participant> _participantRepository;
    private readonly IGenericRepository<Message> _messageRepository;
    private readonly IGenericRepository<Conversation> _conversationRepository;

    public GetInboxQueryHandler(
        IGenericRepository<Participant> participantRepository,
        IGenericRepository<Message> messageRepository,
        IGenericRepository<Conversation> conversationRepository)
    {
        _participantRepository = participantRepository;
        _messageRepository = messageRepository;
        _conversationRepository = conversationRepository;
    }

    public async Task<List<InboxItemDto>> Handle(GetInboxQuery request, CancellationToken cancellationToken)
    {
        var participants = await _participantRepository.FindAsync(p => p.UserId == request.UserId);

        var result = new List<InboxItemDto>();

        foreach (var p in participants)
        {
            var conversation = await _conversationRepository.GetByIdAsync(p.ConversationId);
            if (conversation == null) continue;

            var messages = await _messageRepository.FindAsync(m => m.ConversationId == p.ConversationId);
            var lastMsg = messages.OrderByDescending(m => m.CreatedAt).FirstOrDefault();

            result.Add(new InboxItemDto(
                p.ConversationId,
                conversation.Title,
                conversation.AvatarUrl,
                conversation.Type,
                lastMsg?.Content ?? "Chưa có tin nhắn",
                lastMsg?.CreatedAt,
                0 // UnreadCount (giữ đơn giản MVP)
            ));
        }

        // Sắp xếp theo tin nhắn mới nhất lên đầu
        return result.OrderByDescending(x => x.LastMessageAt).ToList();
    }
}
