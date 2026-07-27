using ChatApp.Application.Interfaces.Repositories;
using ChatApp.Domain.Entities;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace ChatApp.Application.Features.Pinboard.Commands.UpdatePinboardItemContent;

public class UpdatePinboardItemContentCommandHandler : IRequestHandler<UpdatePinboardItemContentCommand, bool>
{
    private readonly IGenericRepository<PinboardItem> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdatePinboardItemContentCommandHandler(IGenericRepository<PinboardItem> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(UpdatePinboardItemContentCommand request, CancellationToken cancellationToken)
    {
        var item = await _repository.GetByIdAsync(request.Id);
        if (item == null) return false;

        item.Content = request.Content;

        _repository.Update(item);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}
