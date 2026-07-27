using ChatApp.Shared.DTOs.Pinboard;
using MediatR;

namespace ChatApp.Application.Features.Pinboard.Queries.GetItemsByConversation;

public record GetItemsByConversationQuery(Guid ConversationId) : IRequest<List<PinboardItemDto>>;
