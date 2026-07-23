using ChatApp.Shared.DTOs.Auth;
using MediatR;

namespace ChatApp.Application.Features.Auth.Commands.ApproveQrSession;

/// <summary>
/// UC-21: App Mobile gọi command này để phê duyệt một QR Session đang chờ.
/// ApprovedByUserId là UserId của người dùng đang dùng App Mobile (đã authenticated).
/// </summary>
public record ApproveQrSessionCommand(
    string SessionId,
    Guid ApprovedByUserId
) : IRequest<ApproveQrSessionResult>;

public record ApproveQrSessionResult(
    bool Success,
    string Message,
    /// <summary>
    /// JWT được cấp cho Web/Desktop client đang chờ QR xác nhận
    /// </summary>
    AuthResponse? AuthResponse = null
);
