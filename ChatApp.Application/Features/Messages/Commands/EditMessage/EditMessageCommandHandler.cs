using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ChatApp.Application.DTOs.Messages;
using ChatApp.Application.Interfaces.Repositories;
using ChatApp.Domain.Entities;
using MediatR;

namespace ChatApp.Application.Features.Messages.Commands.EditMessage;

public class EditMessageCommandHandler : IRequestHandler<EditMessageCommand, MessageDto>
{
    private static readonly TimeSpan EditWindow = TimeSpan.FromMinutes(15);

    private readonly IGenericRepository<Message> _messageRepository;
    private readonly IGenericRepository<Attachment> _attachmentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public EditMessageCommandHandler(
        IGenericRepository<Message> messageRepository,
        IGenericRepository<Attachment> attachmentRepository,
        IUnitOfWork unitOfWork)
    {
        _messageRepository = messageRepository;
        _attachmentRepository = attachmentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<MessageDto> Handle(EditMessageCommand request, CancellationToken cancellationToken)
    {
        var message = await _messageRepository.GetByIdAsync(request.MessageId)
            ?? throw new InvalidOperationException("Message not found.");

        if (message.IsDeleted)
            throw new InvalidOperationException("Cannot edit a deleted message.");

        if (message.SenderId != request.UserId)
            throw new UnauthorizedAccessException("Only the sender can edit this message.");

        if (string.IsNullOrWhiteSpace(request.NewContent))
            throw new InvalidOperationException("Message content cannot be empty.");

        if (DateTime.UtcNow - message.CreatedAt > EditWindow)
            throw new InvalidOperationException($"Messages can only be edited within {EditWindow.TotalMinutes} minutes.");

        message.Content = request.NewContent.Trim();
        message.UpdatedAt = DateTime.UtcNow;

        _messageRepository.Update(message);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var attachment = (await _attachmentRepository.FindAsync(a => a.MessageId == message.Id)).FirstOrDefault();

        return new MessageDto(
            message.Id,
            message.ConversationId,
            message.SenderId,
            message.MessageType,
            message.Content,
            message.CreatedAt,
            attachment?.FileUrl,
            attachment?.FileName,
            message.IsPinned,
            message.IsDeleted,
            message.UpdatedAt
        );
    }
}
