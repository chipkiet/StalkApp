using MediatR;

namespace ChatApp.Application.Features.Friends.Commands.AcceptFriendRequest;

/// <summary>Chấp nhận lời mời kết bạn — chỉ Addressee mới được chấp nhận</summary>
public record AcceptFriendRequestCommand(Guid FriendshipId, Guid CurrentUserId)
    : IRequest<FriendCommandResult>;

public record FriendCommandResult(bool Success, string Message);
