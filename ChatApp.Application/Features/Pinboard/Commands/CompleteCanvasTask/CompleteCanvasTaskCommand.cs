using System;
using System.Threading;
using System.Threading.Tasks;
using ChatApp.Application.Interfaces.Repositories;
using ChatApp.Domain.Entities;
using ChatApp.Shared.DTOs.Pinboard;
using ChatApp.Shared.DTOs.Users;
using MediatR;

namespace ChatApp.Application.Features.Pinboard.Commands.CompleteCanvasTask;

public record CompleteCanvasTaskCommand(Guid TaskId, Guid UserId) : IRequest<KarmaUpdateDto?>;

public class CompleteCanvasTaskCommandHandler : IRequestHandler<CompleteCanvasTaskCommand, KarmaUpdateDto?>
{
    private readonly IGenericRepository<PinboardItem> _pinboardRepository;
    private readonly IGenericRepository<User> _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CompleteCanvasTaskCommandHandler(
        IGenericRepository<PinboardItem> pinboardRepository, 
        IGenericRepository<User> userRepository, 
        IUnitOfWork unitOfWork)
    {
        _pinboardRepository = pinboardRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<KarmaUpdateDto?> Handle(CompleteCanvasTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _pinboardRepository.GetByIdAsync(request.TaskId);
        if (task == null || task.IsCompleted || task.Type != Shared.Enums.PinboardItemType.Task)
            return null;

        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user == null) return null;

        // Gamification logic
        task.IsCompleted = true;
        task.CompletedAt = DateTime.UtcNow;

        int addedPoints = 50; // Base points for completing a task
        if (task.Deadline.HasValue && task.Deadline.Value > DateTime.UtcNow)
        {
            addedPoints += 20; // Bonus for early completion
        }
        
        user.KarmaPoints += addedPoints;

        // Title progression logic
        string? newTitle = null;
        if (user.KarmaPoints > 1000 && user.GamificationTitle != "Kẻ hủy diệt Deadline")
        {
            newTitle = "Kẻ hủy diệt Deadline";
            user.GamificationTitle = newTitle;
        }
        else if (user.KarmaPoints > 500 && user.GamificationTitle != "Trùm cày cuốc")
        {
            newTitle = "Trùm cày cuốc";
            user.GamificationTitle = newTitle;
        }

        _pinboardRepository.Update(task);
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new KarmaUpdateDto
        {
            UserId = user.Id,
            AddedPoints = addedPoints,
            TotalKarmaPoints = user.KarmaPoints,
            NewTitle = newTitle
        };
    }
}
