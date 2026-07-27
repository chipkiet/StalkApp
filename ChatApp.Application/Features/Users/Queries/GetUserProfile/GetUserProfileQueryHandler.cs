using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ChatApp.Application.Interfaces.Repositories;
using ChatApp.Application.Interfaces.Services;
using ChatApp.Domain.Entities;
using ChatApp.Shared.DTOs.Users;
using MediatR;

namespace ChatApp.Application.Features.Users.Queries.GetUserProfile;

public class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, UserProfileDto?>
{
    private readonly IGenericRepository<User> _userRepository;
    private readonly IGenericRepository<Friendship> _friendshipRepository;
    private readonly IPresenceTracker _presenceTracker;

    public GetUserProfileQueryHandler(
        IGenericRepository<User> userRepository,
        IGenericRepository<Friendship> friendshipRepository,
        IPresenceTracker presenceTracker)
    {
        _userRepository = userRepository;
        _friendshipRepository = friendshipRepository;
        _presenceTracker = presenceTracker;
    }

    public async Task<UserProfileDto?> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        var targetUser = await _userRepository.GetByIdAsync(request.TargetUserId);
        if (targetUser == null)
            return null;

        var isOnline = await _presenceTracker.IsOnlineAsync(targetUser.Id);

        var dto = new UserProfileDto
        {
            Id = targetUser.Id,
            DisplayName = targetUser.DisplayName,
            Username = targetUser.Username,
            PhoneNumber = targetUser.PhoneNumber,
            AvatarUrl = targetUser.AvatarUrl,
            Bio = targetUser.Bio,
            IsOnline = isOnline,
            LastSeenAt = isOnline ? null : targetUser.LastSeenAt
        };

        if (request.TargetUserId != request.CurrentUserId)
        {
            var friendships = await _friendshipRepository.FindAsync(f =>
                (f.RequesterId == request.CurrentUserId && f.AddresseeId == request.TargetUserId) ||
                (f.RequesterId == request.TargetUserId && f.AddresseeId == request.CurrentUserId));
            
            var friendship = friendships.FirstOrDefault();
            
            if (friendship != null)
            {
                dto.FriendshipStatus = friendship.Status;
                dto.IsRequester = friendship.RequesterId == request.CurrentUserId;
            }
        }

        return dto;
    }
}
