using ChatApp.Application.Interfaces.Repositories;
using ChatApp.Domain.Entities;
using ChatApp.Shared.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Application.Features.Friends.Commands.SendFriendRequest;

public class SendFriendRequestCommandHandler : IRequestHandler<SendFriendRequestCommand, FriendCommandResult>
{
    private readonly IGenericRepository<Friendship> _friendshipRepo;
    private readonly IGenericRepository<User> _userRepo;
    private readonly IUnitOfWork _unitOfWork;

    public SendFriendRequestCommandHandler(
        IGenericRepository<Friendship> friendshipRepo,
        IGenericRepository<User> userRepo,
        IUnitOfWork unitOfWork)
    {
        _friendshipRepo = friendshipRepo;
        _userRepo = userRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<FriendCommandResult> Handle(SendFriendRequestCommand request, CancellationToken cancellationToken)
    {
        // Không tự kết bạn với chính mình
        if (request.RequesterId == request.AddresseeId)
            return new FriendCommandResult(false, "Không thể tự gửi lời mời kết bạn cho chính mình.");

        // Kiểm tra addressee tồn tại
        var addressee = await _userRepo.GetByIdAsync(request.AddresseeId);
        if (addressee is null)
            return new FriendCommandResult(false, "Người dùng không tồn tại.");

        // Kiểm tra đã có quan hệ chưa (cả 2 chiều)
        var existing = await _friendshipRepo.GetQueryable()
            .FirstOrDefaultAsync(f =>
                (f.RequesterId == request.RequesterId && f.AddresseeId == request.AddresseeId) ||
                (f.RequesterId == request.AddresseeId && f.AddresseeId == request.RequesterId),
                cancellationToken);

        if (existing is not null)
        {
            return existing.Status switch
            {
                FriendshipStatus.Accepted => new FriendCommandResult(false, "Hai bạn đã là bạn bè rồi."),
                FriendshipStatus.Pending => new FriendCommandResult(false, "Lời mời kết bạn đã được gửi, đang chờ phản hồi."),
                FriendshipStatus.Blocked => new FriendCommandResult(false, "Không thể thực hiện hành động này."),
                _ => new FriendCommandResult(false, "Đã tồn tại quan hệ giữa hai người dùng.")
            };
        }

        var friendship = new Friendship
        {
            Id = Guid.NewGuid(),
            RequesterId = request.RequesterId,
            AddresseeId = request.AddresseeId,
            Status = FriendshipStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await _friendshipRepo.AddAsync(friendship);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new FriendCommandResult(true, $"Đã gửi lời mời kết bạn đến {addressee.DisplayName}.", friendship.Id);
    }
}
