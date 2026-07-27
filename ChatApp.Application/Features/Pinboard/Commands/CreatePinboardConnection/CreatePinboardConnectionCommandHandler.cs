using ChatApp.Application.Interfaces.Repositories;
using ChatApp.Domain.Entities;
using ChatApp.Shared.DTOs.Pinboard;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ChatApp.Application.Features.Pinboard.Commands.CreatePinboardConnection;

public class CreatePinboardConnectionCommandHandler : IRequestHandler<CreatePinboardConnectionCommand, PinboardConnectionDto>
{
    private readonly IGenericRepository<PinboardConnection> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreatePinboardConnectionCommandHandler(IGenericRepository<PinboardConnection> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<PinboardConnectionDto> Handle(CreatePinboardConnectionCommand request, CancellationToken cancellationToken)
    {
        var connection = new PinboardConnection
        {
            Id = Guid.NewGuid(),
            ConversationId = request.ConversationId,
            SourceItemId = request.SourceItemId,
            TargetItemId = request.TargetItemId,
            Label = request.Label,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(connection);
        await _unitOfWork.SaveChangesAsync();

        return new PinboardConnectionDto
        {
            Id = connection.Id,
            ConversationId = connection.ConversationId,
            SourceItemId = connection.SourceItemId,
            TargetItemId = connection.TargetItemId,
            Label = connection.Label
        };
    }
}
