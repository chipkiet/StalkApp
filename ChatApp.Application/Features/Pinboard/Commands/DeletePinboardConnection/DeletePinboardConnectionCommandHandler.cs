using ChatApp.Application.Interfaces.Repositories;
using ChatApp.Domain.Entities;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace ChatApp.Application.Features.Pinboard.Commands.DeletePinboardConnection;

public class DeletePinboardConnectionCommandHandler : IRequestHandler<DeletePinboardConnectionCommand, bool>
{
    private readonly IGenericRepository<PinboardConnection> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeletePinboardConnectionCommandHandler(IGenericRepository<PinboardConnection> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeletePinboardConnectionCommand request, CancellationToken cancellationToken)
    {
        var connection = await _repository.GetByIdAsync(request.Id);
        if (connection == null) return false;

        _repository.Remove(connection);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}
