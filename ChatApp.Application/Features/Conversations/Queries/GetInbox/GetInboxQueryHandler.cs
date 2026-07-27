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
    private readonly IGenericRepository<User> _userRepository;

    public GetInboxQueryHandler(
        IGenericRepository<Participant> participantRepository,
        IGenericRepository<Message> messageRepository,
        IGenericRepository<Conversation> conversationRepository,
        IGenericRepository<User> userRepository)
    {
        _participantRepository = participantRepository;
        _messageRepository = messageRepository;
        _conversationRepository = conversationRepository;
        _userRepository = userRepository;
    }

    public async Task<List<InboxItemDto>> Handle(GetInboxQuery request, CancellationToken cancellationToken)
    {
        var participants = await _participantRepository.FindAsync(p => p.UserId == request.UserId && !p.HasDeleted);

        var result = new List<InboxItemDto>();

        foreach (var p in participants)
        {
            var conversation = await _conversationRepository.GetByIdAsync(p.ConversationId);
            if (conversation == null) continue;

            var messages = await _messageRepository.FindAsync(m => m.ConversationId == p.ConversationId);
            if (p.ClearedAt.HasValue)
            {
                messages = messages.Where(m => m.CreatedAt >= p.ClearedAt.Value);
            }
            var lastMsg = messages.OrderByDescending(m => m.CreatedAt).FirstOrDefault();

            string? displayTitle = conversation.Title;
            string? displayAvatar = conversation.AvatarUrl;

            // Nếu là chat 1-1, lấy tên và avatar của người kia
            if (conversation.Type == ChatApp.Domain.Enums.ConversationType.Direct)
            {
                var others = await _participantRepository.FindAsync(x => x.ConversationId == conversation.Id && x.UserId != request.UserId);
                var otherParticipant = others.FirstOrDefault();
                if (otherParticipant != null)
                {
                    var otherUser = await _userRepository.GetByIdAsync(otherParticipant.UserId);
                    if (otherUser != null)
                    {
                        displayTitle = otherUser.DisplayName ?? otherUser.PhoneNumber;
                        displayAvatar = otherUser.AvatarUrl;
                    }
                }
                else 
                {
                    // Trường hợp tự chat với chính mình (Saved Messages)
                    var me = await _userRepository.GetByIdAsync(request.UserId);
                    if (me != null)
                    {
                        displayTitle = me.DisplayName ?? me.PhoneNumber;
                        displayAvatar = me.AvatarUrl;
                    }
                }
            }

            result.Add(new InboxItemDto(
                p.ConversationId,
                displayTitle,
                displayAvatar,
                conversation.Type,
                lastMsg?.Content ?? "Chưa có tin nhắn",
                lastMsg?.CreatedAt,
                0, // UnreadCount (giữ đơn giản MVP)
                p.IsMuted // Truyền IsMuted
            ));
        }

        // Sắp xếp theo tin nhắn mới nhất lên đầu
        return result.OrderByDescending(x => x.LastMessageAt).ToList();
    }
}
