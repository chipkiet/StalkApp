using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ChatApp.Application.DTOs.Messages;
using ChatApp.Application.Interfaces.Repositories;
using ChatApp.Domain.Entities;
using MediatR;

namespace ChatApp.Application.Features.Messages.Commands.DeleteMessage;

public class DeleteMessageCommandHandler : IRequestHandler<DeleteMessageCommand, MessageDto>
{
    private readonly IGenericRepository<Message> _messageRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteMessageCommandHandler(
        IGenericRepository<Message> messageRepository,
        IUnitOfWork unitOfWork)
    {
        _messageRepository = messageRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<MessageDto> Handle(DeleteMessageCommand request, CancellationToken cancellationToken)
    {
        var message = await _messageRepository.GetByIdAsync(request.MessageId)
            ?? throw new InvalidOperationException("Message not found.");

        if (message.SenderId != request.UserId)
            throw new UnauthorizedAccessException("Only the sender can delete this message for everyone.");

        if (message.IsDeleted)
            throw new InvalidOperationException("Message is already deleted.");

        message.IsDeleted = true;
        message.Content = null;
        message.UpdatedAt = DateTime.UtcNow;

        _messageRepository.Update(message);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new MessageDto(
            message.Id,
            message.ConversationId,
            message.SenderId,
            message.MessageType,
            null,
            message.CreatedAt,
            null,
            null,
            message.IsPinned,
            true,
            message.UpdatedAt
        );
    }
}
