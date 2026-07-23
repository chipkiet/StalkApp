using ChatApp.Application.Interfaces.Repositories;
using ChatApp.Application.Interfaces.Services;
using ChatApp.Domain.Entities;
using ChatApp.Shared.DTOs.Auth;
using MediatR;

namespace ChatApp.Application.Features.Auth.Commands.VerifyOtp;

public class VerifyOtpCommandHandler : IRequestHandler<VerifyOtpCommand, AuthResponse>
{
    private readonly IOtpService _otpService;
    private readonly ITokenService _tokenService;
    private readonly IGenericRepository<User> _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public VerifyOtpCommandHandler(
        IOtpService otpService,
        ITokenService tokenService,
        IGenericRepository<User> userRepository,
        IUnitOfWork unitOfWork)
    {
        _otpService = otpService;
        _tokenService = tokenService;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<AuthResponse> Handle(VerifyOtpCommand request, CancellationToken cancellationToken)
    {
        // 1. Xác minh OTP
        var isValid = await _otpService.VerifyOtpAsync(request.PhoneNumber, request.OtpCode);
        if (!isValid)
            throw new UnauthorizedAccessException("OTP không hợp lệ hoặc đã hết hạn.");

        // 2. Chuẩn bị tokens và thời gian
        var refreshToken = _tokenService.GenerateRefreshToken();
        var now = DateTime.UtcNow;

        // 3. Tìm user trong Database theo SĐT
        var existingUsers = await _userRepository.FindAsync(u => u.PhoneNumber == request.PhoneNumber);
        var user = existingUsers.FirstOrDefault();

        bool isNewUser = false;

        if (user is null)
        {
            // UC-02: Tạo tài khoản mới
            // ⚠ Set TẤT CẢ fields TRƯỚC khi AddAsync — KHÔNG gọi Update() sau đó
            // vì Update() trên entity chưa INSERT sẽ gây DbUpdateConcurrencyException
            isNewUser = true;
            user = new User
            {
                Id = Guid.NewGuid(),
                PhoneNumber = request.PhoneNumber,
                Username = $"user_{Guid.NewGuid().ToString("N")[..8]}",
                DisplayName = request.PhoneNumber,
                RefreshToken = refreshToken,
                RefreshTokenExpiryTime = now.AddDays(30),
                CreatedAt = now,
                UpdatedAt = now,
                IsOnline = false
            };
            await _userRepository.AddAsync(user);
        }
        else
        {
            // UC-01: User đã tồn tại — modify trực tiếp entity đang được EF track
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = now.AddDays(30);
            user.UpdatedAt = now;
            _userRepository.Update(user);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 4. Tạo Access Token sau khi đã có Id chắc chắn
        var accessToken = _tokenService.GenerateAccessToken(user);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiry = now.AddMinutes(_tokenService.AccessTokenExpiryMinutes),
            UserId = user.Id,
            Username = user.Username,
            DisplayName = user.DisplayName,
            AvatarUrl = user.AvatarUrl,
            PhoneNumber = user.PhoneNumber ?? string.Empty,
            IsNewUser = isNewUser
        };
    }
}
