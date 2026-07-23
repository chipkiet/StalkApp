using ChatApp.Shared.DTOs.Auth;
using MediatR;

namespace ChatApp.Application.Features.Auth.Commands.VerifyOtp;

/// <summary>
/// UC-01: Login – UC-02: Register. Xác minh OTP, tạo/tìm user, trả JWT.
/// </summary>
public record VerifyOtpCommand(
    string PhoneNumber,
    string OtpCode
) : IRequest<AuthResponse>;
