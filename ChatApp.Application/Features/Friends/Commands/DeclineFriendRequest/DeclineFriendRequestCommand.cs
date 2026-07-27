using MediatR;

namespace ChatApp.Application.Features.Friends.Commands.DeclineFriendRequest;

/// <summary>Từ chối lời mời kết bạn — chỉ Addressee mới được từ chối</summary>
public record DeclineFriendRequestCommand(Guid FriendshipId, Guid CurrentUserId)
    : IRequest<FriendCommandResult>;

public record FriendCommandResult(bool Success, string Message);
