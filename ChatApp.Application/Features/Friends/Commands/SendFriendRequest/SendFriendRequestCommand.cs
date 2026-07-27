using MediatR;

namespace ChatApp.Application.Features.Friends.Commands.SendFriendRequest;

public record SendFriendRequestCommand(Guid RequesterId, Guid AddresseeId)
    : IRequest<FriendCommandResult>;

public record FriendCommandResult(bool Success, string Message, Guid? FriendshipId = null);
