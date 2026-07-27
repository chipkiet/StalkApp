using ChatApp.Shared.DTOs.Friends;
using MediatR;

namespace ChatApp.Application.Features.Friends.Queries.GetPendingRequests;

/// <summary>
/// Lấy danh sách lời mời kết bạn đang chờ (Status = Pending) gửi đến user hiện tại
/// </summary>
public record GetPendingRequestsQuery(Guid UserId) : IRequest<List<FriendDto>>;
