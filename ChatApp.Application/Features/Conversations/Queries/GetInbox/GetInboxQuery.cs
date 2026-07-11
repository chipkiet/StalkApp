using System;
using System.Collections.Generic;
using ChatApp.Application.DTOs.Conversations;
using MediatR;

namespace ChatApp.Application.Features.Conversations.Queries.GetInbox;

public record GetInboxQuery(Guid UserId) : IRequest<List<InboxItemDto>>;
