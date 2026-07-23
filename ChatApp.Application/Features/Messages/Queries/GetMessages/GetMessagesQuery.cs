using System;
using System.Collections.Generic;
using ChatApp.Application.DTOs.Messages;
using MediatR;

namespace ChatApp.Application.Features.Messages.Queries.GetMessages;

// Truyền pageSize và pageIndex cho phân trang sau này, MVP tạm lấy top n tin
public record GetMessagesQuery(Guid ConversationId, Guid UserId, int Count = 50) : IRequest<List<MessageDto>>;
