using ChatApp.Application.Interfaces.Repositories;
using ChatApp.Domain.Entities;
using ChatApp.Shared.Enums;
using MediatR;

namespace ChatApp.Application.Features.Friends.Commands.AcceptFriendRequest;

public class AcceptFriendRequestCommandHandler : IRequestHandler<AcceptFriendRequestCommand, FriendCommandResult>
{
    private readonly IGenericRepository<Friendship> _friendshipRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;

    public AcceptFriendRequestCommandHandler(
        IGenericRepository<Friendship> friendshipRepo,
        IUnitOfWork unitOfWork,
        IMediator mediator)
    {
        _friendshipRepo = friendshipRepo;
        _unitOfWork = unitOfWork;
        _mediator = mediator;
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

        // Tự động tạo cuộc hội thoại (nếu chưa có) giữa 2 người
        await _mediator.Send(new ChatApp.Application.Features.Conversations.Commands.CreateConversation.CreateConversationCommand(
            CreatorId: request.CurrentUserId,
            Title: null,
            Type: ChatApp.Domain.Enums.ConversationType.Direct,
            ParticipantIds: new List<Guid> { request.CurrentUserId, friendship.RequesterId }
        ), cancellationToken);

        return new FriendCommandResult(true, "Đã chấp nhận lời mời kết bạn.");
    }
}
