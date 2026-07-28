using ChatApp.Domain.Entities;
using ChatApp.Application.Interfaces.Repositories;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Application.Features.Messages.Commands.ScheduleMessage;

public class CreateScheduledMessageCommandHandler : IRequestHandler<CreateScheduledMessageCommand, Guid>
{
    private readonly IGenericRepository<ScheduledMessage> _scheduleRepo;
    private readonly IGenericRepository<Participant> _participantRepo;
    private readonly IUnitOfWork _unitOfWork;

    public CreateScheduledMessageCommandHandler(
        IGenericRepository<ScheduledMessage> scheduleRepo,
        IGenericRepository<Participant> participantRepo,
        IUnitOfWork unitOfWork)
    {
        _scheduleRepo = scheduleRepo;
        _participantRepo = participantRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateScheduledMessageCommand request, CancellationToken cancellationToken)
    {
        // Verify conversation and sender existence
        var query = await _participantRepo.FindAsync(p => p.ConversationId == request.ConversationId && p.UserId == request.SenderId);
        var isParticipant = query.Any();
            
        if (!isParticipant)
        {
            throw new UnauthorizedAccessException("User is not a participant in this conversation.");
        }

        var scheduledMessage = new ScheduledMessage
        {
            Id = Guid.NewGuid(),
            ConversationId = request.ConversationId,
            SenderId = request.SenderId,
            Content = request.Content,
            ScheduledAt = request.ScheduledAt.ToUniversalTime(),
            IsSent = false,
            CreatedAt = DateTime.UtcNow
        };

        await _scheduleRepo.AddAsync(scheduledMessage);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return scheduledMessage.Id;
    }
}
