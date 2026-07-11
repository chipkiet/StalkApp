using System;
using System.Threading;
using System.Threading.Tasks;
using ChatApp.Application.Interfaces.Repositories;
using ChatApp.Domain.Entities;
using ChatApp.Domain.Enums;
using MediatR;

namespace ChatApp.Application.Features.Calls.Commands.UpdateCallStatus;

public class UpdateCallStatusCommandHandler : IRequestHandler<UpdateCallStatusCommand, bool>
{
    private readonly IGenericRepository<Call> _callRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCallStatusCommandHandler(IGenericRepository<Call> callRepository, IUnitOfWork unitOfWork)
    {
        _callRepository = callRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(UpdateCallStatusCommand request, CancellationToken cancellationToken)
    {
        var call = await _callRepository.GetByIdAsync(request.CallId);
        if (call == null) return false;

        call.Status = request.Status;

        if (request.Status == CallStatus.Ended || request.Status == CallStatus.Missed || request.Status == CallStatus.Rejected)
        {
            call.EndedAt = DateTime.UtcNow;
        }

        _callRepository.Update(call);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
