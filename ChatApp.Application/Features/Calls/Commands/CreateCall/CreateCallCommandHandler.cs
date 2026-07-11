using System;
using System.Threading;
using System.Threading.Tasks;
using ChatApp.Application.Interfaces.Repositories;
using ChatApp.Domain.Entities;
using ChatApp.Domain.Enums;
using MediatR;

namespace ChatApp.Application.Features.Calls.Commands.CreateCall;

public class CreateCallCommandHandler : IRequestHandler<CreateCallCommand, Guid>
{
    private readonly IGenericRepository<Call> _callRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCallCommandHandler(IGenericRepository<Call> callRepository, IUnitOfWork unitOfWork)
    {
        _callRepository = callRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateCallCommand request, CancellationToken cancellationToken)
    {
        var call = new Call
        {
            Id = request.Id,
            ConversationId = request.ConversationId,
            CallerId = request.CallerId,
            Type = request.Type,
            Status = CallStatus.Ongoing,
            StartedAt = DateTime.UtcNow
        };

        await _callRepository.AddAsync(call);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return call.Id;
    }
}
