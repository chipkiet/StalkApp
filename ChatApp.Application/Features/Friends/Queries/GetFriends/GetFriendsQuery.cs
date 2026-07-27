using ChatApp.Shared.DTOs.Friends;
using MediatR;

namespace ChatApp.Application.Features.Friends.Queries.GetFriends;

/// <summary>
/// Lấy danh sách bạn bè (Status = Accepted) của user hiện tại
/// </summary>
public record GetFriendsQuery(Guid UserId) : IRequest<List<FriendDto>>;
