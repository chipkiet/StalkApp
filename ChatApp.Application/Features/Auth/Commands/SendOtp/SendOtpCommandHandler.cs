using ChatApp.Application.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ChatApp.Application.Features.Auth.Commands.SendOtp;

public class SendOtpCommandHandler : IRequestHandler<SendOtpCommand, SendOtpResult>
{
    private readonly IOtpService _otpService;
    private readonly ILogger<SendOtpCommandHandler> _logger;

    public SendOtpCommandHandler(IOtpService otpService, ILogger<SendOtpCommandHandler> logger)
    {
        _otpService = otpService;
        _logger = logger;
    }

    public async Task<SendOtpResult> Handle(SendOtpCommand request, CancellationToken cancellationToken)
    {
        // Validate phone number format cơ bản
        if (string.IsNullOrWhiteSpace(request.PhoneNumber))
            return new SendOtpResult(false, "Số điện thoại không được để trống.");

        // Tạo và lưu OTP vào MemoryCache (TTL = 3 phút)
        var otpCode = await _otpService.GenerateAndStoreOtpAsync(request.PhoneNumber);

        // Trong môi trường thực tế: gọi SMS Gateway (Twilio, Firebase, v.v.)
        // Hiện tại: LOG ra console để dev xem
        _logger.LogWarning(
            "[DEV-MODE] OTP cho số {Phone}: {OtpCode} (hết hạn sau 3 phút)",
            request.PhoneNumber,
            otpCode
        );

        return new SendOtpResult(
            Success: true,
            Message: $"OTP đã được gửi đến {request.PhoneNumber}. Vui lòng kiểm tra tin nhắn.",
            OtpCodeDev: otpCode // Chỉ dùng trong môi trường Dev
        );
    }
}
