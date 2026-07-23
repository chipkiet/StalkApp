using MediatR;

namespace ChatApp.Application.Features.Auth.Commands.SendOtp;

/// <summary>
/// UC-01/UC-02: Yêu cầu gửi OTP về số điện thoại
/// </summary>
public record SendOtpCommand(string PhoneNumber) : IRequest<SendOtpResult>;

public record SendOtpResult(
    bool Success,
    string Message,
    /// <summary>Chỉ trả về trong môi trường Dev để tiện test</summary>
    string? OtpCodeDev = null
);
