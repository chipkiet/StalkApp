using MediatR;

namespace ChatApp.Application.Features.Friends.Commands.RemoveFriend;

/// <summary>Xóa bạn bè — bất kỳ bên nào cũng có thể thực hiện</summary>
public record RemoveFriendCommand(Guid FriendshipId, Guid CurrentUserId)
    : IRequest<FriendCommandResult>;

public record FriendCommandResult(bool Success, string Message);
