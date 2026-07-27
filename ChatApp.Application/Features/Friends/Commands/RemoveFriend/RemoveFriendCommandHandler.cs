using ChatApp.Application.Interfaces.Repositories;
using ChatApp.Domain.Entities;
using ChatApp.Shared.Enums;
using MediatR;

namespace ChatApp.Application.Features.Friends.Commands.RemoveFriend;

public class RemoveFriendCommandHandler : IRequestHandler<RemoveFriendCommand, FriendCommandResult>
{
    private readonly IGenericRepository<Friendship> _friendshipRepo;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveFriendCommandHandler(
        IGenericRepository<Friendship> friendshipRepo,
        IUnitOfWork unitOfWork)
    {
        _friendshipRepo = friendshipRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<FriendCommandResult> Handle(RemoveFriendCommand request, CancellationToken cancellationToken)
    {
        var friendship = await _friendshipRepo.GetByIdAsync(request.FriendshipId);

        if (friendship is null)
            return new FriendCommandResult(false, "Không tìm thấy mối quan hệ bạn bè.");

        // Chỉ 1 trong 2 người liên quan mới được xóa
        if (friendship.RequesterId != request.CurrentUserId && friendship.AddresseeId != request.CurrentUserId)
            return new FriendCommandResult(false, "Bạn không có quyền thực hiện hành động này.");

        if (friendship.Status != FriendshipStatus.Accepted)
            return new FriendCommandResult(false, "Không có quan hệ bạn bè để xóa.");

        _friendshipRepo.Remove(friendship);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new FriendCommandResult(true, "Đã xóa bạn bè thành công.");
    }
}
