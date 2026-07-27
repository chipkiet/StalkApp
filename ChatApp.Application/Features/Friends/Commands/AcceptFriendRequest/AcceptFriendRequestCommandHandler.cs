using ChatApp.Application.Interfaces.Repositories;
using ChatApp.Domain.Entities;
using ChatApp.Shared.Enums;
using MediatR;

namespace ChatApp.Application.Features.Friends.Commands.AcceptFriendRequest;

public class AcceptFriendRequestCommandHandler : IRequestHandler<AcceptFriendRequestCommand, FriendCommandResult>
{
    private readonly IGenericRepository<Friendship> _friendshipRepo;
    private readonly IUnitOfWork _unitOfWork;

    public AcceptFriendRequestCommandHandler(
        IGenericRepository<Friendship> friendshipRepo,
        IUnitOfWork unitOfWork)
    {
        _friendshipRepo = friendshipRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<FriendCommandResult> Handle(AcceptFriendRequestCommand request, CancellationToken cancellationToken)
    {
        var friendship = await _friendshipRepo.GetByIdAsync(request.FriendshipId);

        if (friendship is null)
            return new FriendCommandResult(false, "Lời mời kết bạn không tồn tại.");

        // Chỉ người nhận mới được chấp nhận
        if (friendship.AddresseeId != request.CurrentUserId)
            return new FriendCommandResult(false, "Bạn không có quyền thực hiện hành động này.");

        if (friendship.Status != FriendshipStatus.Pending)
            return new FriendCommandResult(false, "Lời mời này không ở trạng thái chờ.");

        friendship.Status = FriendshipStatus.Accepted;
        friendship.UpdatedAt = DateTime.UtcNow;

        _friendshipRepo.Update(friendship);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new FriendCommandResult(true, "Đã chấp nhận lời mời kết bạn.");
    }
}
