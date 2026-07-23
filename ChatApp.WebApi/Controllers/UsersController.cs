using System.Security.Claims;
using ChatApp.Application.Features.Users.Commands.UpdateProfile;
using ChatApp.Application.Interfaces.Services;
using ChatApp.Application.Interfaces.Repositories;
using ChatApp.Domain.Entities;
using ChatApp.Shared.DTOs.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Threading.Tasks;
using ChatApp.Application.Interfaces.Repositories;
using ChatApp.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Tất cả endpoint trong controller này đều cần xác thực
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IPresenceTracker _presenceTracker;
    private readonly IGenericRepository<User> _userRepository;

    public UsersController(
        IMediator mediator,
        IPresenceTracker presenceTracker,
        IGenericRepository<User> userRepository)
    {
        _mediator = mediator;
        _presenceTracker = presenceTracker;
        _userRepository = userRepository;
    }

    /// <summary>
    /// UC-03: Cập nhật thông tin cá nhân của user đang đăng nhập
    /// </summary>
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? User.FindFirst("sub")?.Value 
            ?? User.FindFirst("nameid")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { message = "Token không hợp lệ." });

        if (string.IsNullOrWhiteSpace(request.DisplayName))
            return BadRequest(new { message = "Tên hiển thị không được để trống." });

        var result = await _mediator.Send(new UpdateProfileCommand(
            UserId: userId,
            DisplayName: request.DisplayName,
            Username: request.Username,
            Bio: request.Bio,
            AvatarUrl: request.AvatarUrl
        ));

        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(new
        {
            message = result.Message,
            username = result.Username,
            displayName = result.DisplayName,
            avatarUrl = result.AvatarUrl,
            bio = result.Bio
        });
    }

    /// <summary>
    /// Lấy thông tin cá nhân của user đang đăng nhập
    /// </summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? User.FindFirst("sub")?.Value 
            ?? User.FindFirst("nameid")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { message = "Token không hợp lệ." });

        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            return NotFound(new { message = "Người dùng không tồn tại." });

        return Ok(new
        {
            id = user.Id,
            displayName = user.DisplayName,
            username = user.Username,
            phoneNumber = user.PhoneNumber,
            avatarUrl = user.AvatarUrl,
            bio = user.Bio
        });
    }

    /// <summary>
    /// Lấy thông tin cá nhân của một user bất kỳ theo ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetUserProfile(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user is null)
            return NotFound(new { message = "Người dùng không tồn tại." });

        return Ok(new
        {
            id = user.Id,
            displayName = user.DisplayName,
            username = user.Username,
            phoneNumber = user.PhoneNumber,
            avatarUrl = user.AvatarUrl,
            bio = user.Bio
        });
    }

    /// <summary>
    /// UC-22: Lấy trạng thái online/offline của một user
    /// </summary>
    [HttpGet("{userId}/status")]
    public async Task<IActionResult> GetUserStatus(Guid userId)
    {
        var isOnline = await _presenceTracker.IsOnlineAsync(userId);

        // Lấy LastSeenAt từ Database nếu user offline
        DateTime? lastSeenAt = null;
        if (!isOnline)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user is null)
                return NotFound(new { message = "Người dùng không tồn tại." });

            lastSeenAt = user.LastSeenAt;
        }

        return Ok(new UserStatusDto
        {
            UserId = userId,
            IsOnline = isOnline,
            LastSeenAt = lastSeenAt
        });
    }

    /// <summary>
    /// UC-22: Lấy trạng thái online/offline của nhiều users (batch query)
    /// </summary>
    [HttpPost("status/batch")]
    public async Task<IActionResult> GetUsersStatusBatch([FromBody] List<Guid> userIds)
    {
        var onlineUsers = await _presenceTracker.GetOnlineUsersAsync();
        var onlineSet = onlineUsers.ToHashSet();

        var result = userIds.Select(id => new UserStatusDto
        {
            UserId = id,
            IsOnline = onlineSet.Contains(id),
            LastSeenAt = null // Không query DB cho batch để giữ hiệu năng
        });

        return Ok(result);
    }

    /// <summary>
    /// Tìm kiếm user theo số điện thoại (dùng khi tạo cuộc trò chuyện mới)
    /// </summary>
    [HttpGet("search")]
    public async Task<IActionResult> SearchByPhone([FromQuery] string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return BadRequest(new { message = "Vui lòng nhập số điện thoại." });

        var users = await _userRepository.FindAsync(u => u.PhoneNumber == phone.Trim());
        var user = users.FirstOrDefault();

        if (user is null)
            return NotFound(new { message = "Không tìm thấy người dùng với số điện thoại này." });

        return Ok(new
        {
            id = user.Id,
            displayName = user.DisplayName,
            username = user.Username,
            phoneNumber = user.PhoneNumber,
            avatarUrl = user.AvatarUrl
        });
    }

    /// <summary>Demo accounts

}
