using ChatApp.Application.Interfaces.Repositories;
using ChatApp.Domain.Entities;
using ChatApp.Shared.Enums;
using MediatR;

namespace ChatApp.Application.Features.Friends.Commands.DeclineFriendRequest;

public class DeclineFriendRequestCommandHandler : IRequestHandler<DeclineFriendRequestCommand, FriendCommandResult>
{
    private readonly IGenericRepository<Friendship> _friendshipRepo;
    private readonly IUnitOfWork _unitOfWork;

    public DeclineFriendRequestCommandHandler(
        IGenericRepository<Friendship> friendshipRepo,
        IUnitOfWork unitOfWork)
    {
        _friendshipRepo = friendshipRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<FriendCommandResult> Handle(DeclineFriendRequestCommand request, CancellationToken cancellationToken)
    {
        var friendship = await _friendshipRepo.GetByIdAsync(request.FriendshipId);

        if (friendship is null)
            return new FriendCommandResult(false, "Lời mời kết bạn không tồn tại.");

        // Chỉ người nhận mới được từ chối
        if (friendship.AddresseeId != request.CurrentUserId)
            return new FriendCommandResult(false, "Bạn không có quyền thực hiện hành động này.");

        if (friendship.Status != FriendshipStatus.Pending)
            return new FriendCommandResult(false, "Lời mời này không ở trạng thái chờ.");

        // Xóa bản ghi để cả 2 người có thể gửi lại lời mời sau
        _friendshipRepo.Remove(friendship);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new FriendCommandResult(true, "Đã từ chối lời mời kết bạn.");
    }
}
