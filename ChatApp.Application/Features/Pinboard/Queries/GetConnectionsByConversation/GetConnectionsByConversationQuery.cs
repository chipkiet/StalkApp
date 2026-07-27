using ChatApp.Shared.DTOs.Pinboard;
using MediatR;
using System.Collections.Generic;
using System;

namespace ChatApp.Application.Features.Pinboard.Queries.GetConnectionsByConversation;

public record GetConnectionsByConversationQuery(Guid ConversationId) : IRequest<List<PinboardConnectionDto>>;
