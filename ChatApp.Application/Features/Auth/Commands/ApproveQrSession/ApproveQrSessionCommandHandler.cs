using ChatApp.Application.Interfaces.Repositories;
using ChatApp.Application.Interfaces.Services;
using ChatApp.Domain.Entities;
using ChatApp.Shared.DTOs.Auth;
using MediatR;
using Microsoft.Extensions.Caching.Memory;

namespace ChatApp.Application.Features.Auth.Commands.ApproveQrSession;

public class ApproveQrSessionCommandHandler : IRequestHandler<ApproveQrSessionCommand, ApproveQrSessionResult>
{
    private readonly IMemoryCache _cache;
    private readonly ITokenService _tokenService;
    private readonly IGenericRepository<User> _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    // Cache key prefix cho QR Session
    public const string QR_SESSION_PREFIX = "QR_SESSION_";

    public ApproveQrSessionCommandHandler(
        IMemoryCache cache,
        ITokenService tokenService,
        IGenericRepository<User> userRepository,
        IUnitOfWork unitOfWork)
    {
        _cache = cache;
        _tokenService = tokenService;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ApproveQrSessionResult> Handle(ApproveQrSessionCommand request, CancellationToken cancellationToken)
    {
        var cacheKey = QR_SESSION_PREFIX + request.SessionId;

        // 1. Kiểm tra QR Session có tồn tại và chưa được dùng không
        if (!_cache.TryGetValue(cacheKey, out QrSessionState? sessionState) || sessionState == null)
            return new ApproveQrSessionResult(false, "QR Code không hợp lệ hoặc đã hết hạn.");

        if (sessionState.IsApproved)
            return new ApproveQrSessionResult(false, "QR Code này đã được sử dụng.");

        // 2. Tìm user đang approve (Mobile user)
        var user = await _userRepository.GetByIdAsync(request.ApprovedByUserId);
        if (user is null)
            return new ApproveQrSessionResult(false, "Người dùng không tồn tại.");

        // 3. Cấp token mới cho phiên Web/Desktop
        var newRefreshToken = _tokenService.GenerateRefreshToken();
        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(30);
        user.UpdatedAt = DateTime.UtcNow;
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var accessToken = _tokenService.GenerateAccessToken(user);

        var authResponse = new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken,
            AccessTokenExpiry = DateTime.UtcNow.AddMinutes(_tokenService.AccessTokenExpiryMinutes),
            UserId = user.Id,
            Username = user.Username,
            DisplayName = user.DisplayName,
            AvatarUrl = user.AvatarUrl,
            PhoneNumber = user.PhoneNumber ?? string.Empty,
            IsNewUser = false
        };

        // 4. Đánh dấu QR Session đã được approve và lưu AuthResponse để Hub đẩy về
        sessionState.IsApproved = true;
        sessionState.AuthResponse = authResponse;
        _cache.Set(cacheKey, sessionState, TimeSpan.FromMinutes(1)); // Cho thêm 1 phút để Hub đọc

        return new ApproveQrSessionResult(true, "Xác nhận thành công.", authResponse);
    }
}

/// <summary>
/// Trạng thái nội bộ của một QR Session trong MemoryCache
/// </summary>
public class QrSessionState
{
    public string SessionId { get; set; } = string.Empty;
    public bool IsApproved { get; set; }
    public AuthResponse? AuthResponse { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
