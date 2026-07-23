using System.Security.Claims;
using ChatApp.Application.Features.Auth.Commands.ApproveQrSession;
using ChatApp.Application.Features.Auth.Commands.SendOtp;
using ChatApp.Application.Features.Auth.Commands.VerifyOtp;
using ChatApp.Shared.DTOs.Auth;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace ChatApp.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IMemoryCache _cache;

    public AuthController(IMediator mediator, IMemoryCache cache)
    {
        _mediator = mediator;
        _cache = cache;
    }

    /// <summary>
    /// UC-01/UC-02: Bước 1 – Gửi OTP về số điện thoại
    /// </summary>
    [HttpPost("send-otp")]
    [AllowAnonymous]
    public async Task<IActionResult> SendOtp([FromBody] SendOtpRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.PhoneNumber))
            return BadRequest(new { message = "Số điện thoại không được để trống." });

        var result = await _mediator.Send(new SendOtpCommand(request.PhoneNumber));

        if (!result.Success)
            return BadRequest(new { message = result.Message });

        // Trong môi trường DEV: trả OTP về client để test dễ dàng
        // Trong Production: không trả OtpCodeDev
        return Ok(new
        {
            message = result.Message,
            otpCodeDev = result.OtpCodeDev // Xoá key này khi deploy Production
        });
    }

    /// <summary>
    /// UC-01: Login / UC-02: Register – Bước 2: Xác minh OTP và nhận JWT
    /// </summary>
    [HttpPost("verify-otp")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
    {
        try
        {
            var authResponse = await _mediator.Send(new VerifyOtpCommand(request.PhoneNumber, request.OtpCode));
            return Ok(authResponse);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    /// <summary>
    /// UC-21: Bước 1 – Web/Desktop tạo QR Session và nhận SessionId
    /// </summary>
    [HttpGet("qr/generate")]
    [AllowAnonymous]
    public IActionResult GenerateQrSession()
    {
        var sessionId = Guid.NewGuid().ToString("N");
        var expiresAt = DateTime.UtcNow.AddMinutes(3);

        // Lưu trạng thái ban đầu của QR Session vào MemoryCache (TTL = 3 phút)
        var sessionState = new QrSessionState
        {
            SessionId = sessionId,
            IsApproved = false,
            CreatedAt = DateTime.UtcNow
        };

        _cache.Set(
            ApproveQrSessionCommandHandler.QR_SESSION_PREFIX + sessionId,
            sessionState,
            expiresAt
        );

        return Ok(new QrSessionResponse
        {
            SessionId = sessionId,
            ExpiresAt = expiresAt
        });
    }

    /// <summary>
    /// UC-21: Bước 2 – App Mobile (đã authenticated) xác nhận QR Session
    /// Backend sẽ gửi JWT về Web/Desktop qua SignalR
    /// </summary>
    [HttpPost("qr/approve")]
    [Authorize]
    public async Task<IActionResult> ApproveQrSession([FromBody] ApproveQrRequest request)
    {
        // Lấy UserId từ JWT Claims của Mobile App
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? User.FindFirst("sub")?.Value 
            ?? User.FindFirst("nameid")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { message = "Token không hợp lệ." });

        var result = await _mediator.Send(new ApproveQrSessionCommand(request.SessionId, userId));

        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(new { message = result.Message });
    }
}

/// <summary>
/// Request body cho API approve QR
/// </summary>
public record ApproveQrRequest(string SessionId);
