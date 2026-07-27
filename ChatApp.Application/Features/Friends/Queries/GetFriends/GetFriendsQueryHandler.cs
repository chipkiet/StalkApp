using ChatApp.Application.Interfaces.Repositories;
using ChatApp.Application.Interfaces.Services;
using ChatApp.Domain.Entities;
using ChatApp.Shared.DTOs.Friends;
using ChatApp.Shared.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Application.Features.Friends.Queries.GetFriends;

public class GetFriendsQueryHandler : IRequestHandler<GetFriendsQuery, List<FriendDto>>
{
    private readonly IGenericRepository<Friendship> _friendshipRepo;
    private readonly IPresenceTracker _presenceTracker;

    public GetFriendsQueryHandler(
        IGenericRepository<Friendship> friendshipRepo,
        IPresenceTracker presenceTracker)
    {
        _friendshipRepo = friendshipRepo;
        _presenceTracker = presenceTracker;
    }

    public async Task<List<FriendDto>> Handle(GetFriendsQuery request, CancellationToken cancellationToken)
    {
        // Lấy tất cả friendship có status = Accepted liên quan đến user này
        var friendships = await _friendshipRepo.GetQueryable()
            .Include(f => f.Requester)
            .Include(f => f.Addressee)
            .Where(f =>
                f.Status == FriendshipStatus.Accepted &&
                (f.RequesterId == request.UserId || f.AddresseeId == request.UserId))
            .ToListAsync(cancellationToken);

        // Lấy danh sách online users
        var onlineUsers = await _presenceTracker.GetOnlineUsersAsync();
        var onlineSet = onlineUsers.ToHashSet();

        var result = friendships.Select(f =>
        {
            // Xác định người bạn (người còn lại trong quan hệ)
            var isRequester = f.RequesterId == request.UserId;
            var friend = isRequester ? f.Addressee : f.Requester;

            return new FriendDto
            {
                FriendshipId = f.Id,
                UserId = friend.Id,
                DisplayName = friend.DisplayName,
                Username = friend.Username,
                AvatarUrl = friend.AvatarUrl,
                PhoneNumber = friend.PhoneNumber,
                IsOnline = onlineSet.Contains(friend.Id),
                LastSeenAt = friend.LastSeenAt,
                Status = f.Status,
                IsRequester = isRequester
            };
        })
        .OrderBy(f => f.DisplayName)
        .ToList();

        return result;
    }
}
