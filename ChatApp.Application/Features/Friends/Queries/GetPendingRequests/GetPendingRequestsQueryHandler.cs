using ChatApp.Application.Interfaces.Repositories;
using ChatApp.Domain.Entities;
using ChatApp.Shared.DTOs.Friends;
using ChatApp.Shared.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Application.Features.Friends.Queries.GetPendingRequests;

public class GetPendingRequestsQueryHandler : IRequestHandler<GetPendingRequestsQuery, List<FriendDto>>
{
    private readonly IGenericRepository<Friendship> _friendshipRepo;

    public GetPendingRequestsQueryHandler(IGenericRepository<Friendship> friendshipRepo)
    {
        _friendshipRepo = friendshipRepo;
    }

    public async Task<List<FriendDto>> Handle(GetPendingRequestsQuery request, CancellationToken cancellationToken)
    {
        // Chỉ lấy những lời mời MÀ user hiện tại là người NHẬN (Addressee)
        var pendingRequests = await _friendshipRepo.GetQueryable()
            .Include(f => f.Requester)
            .Where(f =>
                f.Status == FriendshipStatus.Pending &&
                f.AddresseeId == request.UserId)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync(cancellationToken);

        var result = pendingRequests.Select(f => new FriendDto
        {
            FriendshipId = f.Id,
            UserId = f.Requester.Id,
            DisplayName = f.Requester.DisplayName,
            Username = f.Requester.Username,
            AvatarUrl = f.Requester.AvatarUrl,
            PhoneNumber = f.Requester.PhoneNumber,
            IsOnline = false,
            LastSeenAt = null,
            Status = f.Status,
            IsRequester = false   // Đây là người gửi, mình là người nhận
        }).ToList();

        return result;
    }
}
