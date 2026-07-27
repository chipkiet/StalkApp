using ChatApp.Application.Interfaces.Repositories;
using MediatR;

namespace ChatApp.Application.Features.Pinboard.Commands.DeletePinboardItem;

public class DeletePinboardItemCommandHandler : IRequestHandler<DeletePinboardItemCommand, bool>
{
    private readonly IGenericRepository<Domain.Entities.PinboardItem> _repo;
    private readonly IUnitOfWork _unitOfWork;

    public DeletePinboardItemCommandHandler(IGenericRepository<Domain.Entities.PinboardItem> repo, IUnitOfWork unitOfWork)
    {
        _repo = repo;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeletePinboardItemCommand request, CancellationToken cancellationToken)
    {
        var item = await _repo.GetByIdAsync(request.Id);
        
        if (item == null) return false;

        _repo.Remove(item);
        return await _unitOfWork.SaveChangesAsync(cancellationToken) > 0;
    }
}
