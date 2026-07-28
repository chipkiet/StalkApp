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
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? User.FindFirst("sub")?.Value 
            ?? User.FindFirst("nameid")?.Value;
            
        if (!Guid.TryParse(userIdClaim, out var currentUserId))
            return Unauthorized(new { message = "Token không hợp lệ." });

        var query = new ChatApp.Application.Features.Users.Queries.GetUserProfile.GetUserProfileQuery(id, currentUserId);
        var userProfile = await _mediator.Send(query);

        if (userProfile is null)
            return NotFound(new { message = "Người dùng không tồn tại." });

        return Ok(userProfile);
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
    /// Chuẩn hóa số điện thoại Việt Nam về định dạng E.164 (+84...) trước khi
    /// so sánh với dữ liệu trong DB.
    ///
    /// Bảng quy đổi:
    ///   "01234567891"   -> "+841234567891"  (0 -> +84)
    ///   "841234567891"  -> "+841234567891"  (84 không có dấu + -> +84)
    ///   "00841234567891"-> "+841234567891"  (0084 -> +84)
    ///   "+841234567891" -> "+841234567891"  (giữ nguyên)
    ///   Các ký tự khoảng trắng, gạch ngang, dấu chấm được loại bỏ.
    /// </summary>
    private static string NormalizeVietnamesePhone(string raw)
    {
        // Bỏ khoảng trắng, gạch ngang, dấu chấm
        var cleaned = raw.Trim()
                         .Replace(" ", "")
                         .Replace("-", "")
                         .Replace(".", "");

        if (cleaned.StartsWith("0084"))
            return "+84" + cleaned[4..];

        if (cleaned.StartsWith("+84"))
            return cleaned; // đã đúng định dạng

        if (cleaned.StartsWith("84") && cleaned.Length >= 11)
            return "+84" + cleaned[2..];

        if (cleaned.StartsWith("0"))
            return "+84" + cleaned[1..];

        // Fallback: trả về nguyên bản (có thể là số quốc tế khác)
        return cleaned;
    }

    /// <summary>
    /// Tìm kiếm user theo số điện thoại (dùng khi tạo cuộc trò chuyện mới)
    /// </summary>
    [HttpGet("search")]
    public async Task<IActionResult> SearchByPhone([FromQuery] string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return BadRequest(new { message = "Vui lòng nhập số điện thoại." });

        // Chuẩn hóa đầu vào về E.164 trước khi truy vấn DB
        var normalized = NormalizeVietnamesePhone(phone);

        var users = await _userRepository.FindAsync(u => u.PhoneNumber == normalized);
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
