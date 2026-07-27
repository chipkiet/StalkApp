using System.Security.Claims;
using ChatApp.Application.Features.Friends.Commands.AcceptFriendRequest;
using ChatApp.Application.Features.Friends.Commands.DeclineFriendRequest;
using ChatApp.Application.Features.Friends.Commands.RemoveFriend;
using ChatApp.Application.Features.Friends.Commands.SendFriendRequest;
using ChatApp.Application.Features.Friends.Queries.GetFriends;
using ChatApp.Application.Features.Friends.Queries.GetPendingRequests;
using ChatApp.Application.Interfaces.Repositories;
using ChatApp.Domain.Entities;
using ChatApp.Shared.DTOs.Friends;
using ChatApp.Shared.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ChatApp.WebApi.Hubs;

namespace ChatApp.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FriendsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IHubContext<ChatHub> _hubContext;
    private readonly IGenericRepository<User> _userRepository;
    private readonly IGenericRepository<Friendship> _friendshipRepository;

    public FriendsController(
        IMediator mediator,
        IHubContext<ChatHub> hubContext,
        IGenericRepository<User> userRepository,
        IGenericRepository<Friendship> friendshipRepository)
    {
        _mediator = mediator;
        _hubContext = hubContext;
        _userRepository = userRepository;
        _friendshipRepository = friendshipRepository;
    }

    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value
            ?? User.FindFirst("nameid")?.Value;
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }

    /// <summary>
    /// GET /api/friends — Lấy danh sách bạn bè (Status = Accepted)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetFriends()
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var friends = await _mediator.Send(new GetFriendsQuery(userId));
        return Ok(friends);
    }

    /// <summary>
    /// GET /api/friends/pending — Lấy danh sách lời mời đang chờ
    /// </summary>
    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingRequests()
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var requests = await _mediator.Send(new GetPendingRequestsQuery(userId));
        return Ok(requests);
    }

    /// <summary>
    /// POST /api/friends/request — Gửi lời mời kết bạn + push SignalR đến người nhận
    /// </summary>
    [HttpPost("request")]
    public async Task<IActionResult> SendFriendRequest([FromBody] SendFriendRequestDto dto)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var result = await _mediator.Send(new SendFriendRequestCommand(userId, dto.AddresseeId));

        if (!result.Success)
            return BadRequest(new { message = result.Message });

        // ── Push SignalR: thông báo người nhận có lời mời mới ──
        var sender = await _userRepository.GetByIdAsync(userId);
        if (sender != null && result.FriendshipId.HasValue)
        {
            var notification = new FriendNotificationDto
            {
                FriendshipId = result.FriendshipId.Value,
                UserId = userId,
                DisplayName = sender.DisplayName,
                AvatarUrl = sender.AvatarUrl
            };
            await _hubContext.Clients
                .Group($"user_{dto.AddresseeId}")
                .SendAsync("FriendRequestReceived", notification);
        }

        return Ok(new { message = result.Message, friendshipId = result.FriendshipId });
    }

    /// <summary>
    /// PUT /api/friends/{id}/accept — Chấp nhận lời mời + push SignalR đến người gửi
    /// </summary>
    [HttpPut("{id:guid}/accept")]
    public async Task<IActionResult> AcceptFriendRequest(Guid id)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var result = await _mediator.Send(new AcceptFriendRequestCommand(id, userId));

        if (!result.Success)
            return BadRequest(new { message = result.Message });

        // ── Push SignalR: thông báo người gửi request đã được chấp nhận ──
        var friendship = await _friendshipRepository.GetByIdAsync(id);
        var acceptor = await _userRepository.GetByIdAsync(userId);
        if (friendship != null && acceptor != null)
        {
            var notification = new FriendNotificationDto
            {
                FriendshipId = id,
                UserId = userId,
                DisplayName = acceptor.DisplayName,
                AvatarUrl = acceptor.AvatarUrl
            };
            // Thông báo người gửi (Requester)
            await _hubContext.Clients
                .Group($"user_{friendship.RequesterId}")
                .SendAsync("FriendRequestAccepted", notification);

            // Thông báo người chấp nhận (Addressee = current user) để reload danh sách
            await _hubContext.Clients
                .Group($"user_{userId}")
                .SendAsync("FriendRequestAccepted", notification);
        }

        return Ok(new { message = result.Message });
    }

    /// <summary>
    /// PUT /api/friends/{id}/decline — Từ chối lời mời + push SignalR đến người gửi
    /// </summary>
    [HttpPut("{id:guid}/decline")]
    public async Task<IActionResult> DeclineFriendRequest(Guid id)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized();

        // Lấy friendship trước khi xóa để biết RequesterId
        var friendship = await _friendshipRepository.GetByIdAsync(id);

        var result = await _mediator.Send(new DeclineFriendRequestCommand(id, userId));

        if (!result.Success)
            return BadRequest(new { message = result.Message });

        // ── Push SignalR: thông báo người gửi request bị từ chối ──
        if (friendship != null)
        {
            var notification = new FriendNotificationDto
            {
                FriendshipId = id,
                UserId = userId,
                DisplayName = string.Empty
            };
            await _hubContext.Clients
                .Group($"user_{friendship.RequesterId}")
                .SendAsync("FriendRequestDeclined", notification);
        }

        return Ok(new { message = result.Message });
    }

    /// <summary>
    /// DELETE /api/friends/{id} — Xóa bạn bè + push SignalR cho cả 2 phía
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> RemoveFriend(Guid id)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized();

        // Lấy friendship trước khi xóa
        var friendship = await _friendshipRepository.GetByIdAsync(id);

        var result = await _mediator.Send(new RemoveFriendCommand(id, userId));

        if (!result.Success)
            return BadRequest(new { message = result.Message });

        // ── Push SignalR: thông báo cả 2 người để cập nhật UI ──
        if (friendship != null)
        {
            var notification = new FriendNotificationDto
            {
                FriendshipId = id,
                UserId = userId,
                DisplayName = string.Empty
            };
            var otherUserId = friendship.RequesterId == userId
                ? friendship.AddresseeId
                : friendship.RequesterId;

            await _hubContext.Clients
                .Group($"user_{otherUserId}")
                .SendAsync("FriendRemoved", notification);
            await _hubContext.Clients
                .Group($"user_{userId}")
                .SendAsync("FriendRemoved", notification);
        }

        return Ok(new { message = result.Message });
    }
}
