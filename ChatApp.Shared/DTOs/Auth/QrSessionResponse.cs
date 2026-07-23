namespace ChatApp.Shared.DTOs.Auth;

public class QrSessionResponse
{
    /// <summary>
    /// ID phiên QR duy nhất, được encode thành ảnh QR trên client
    /// </summary>
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// Thời điểm QR hết hạn (UTC) - sau 3 phút
    /// </summary>
    public DateTime ExpiresAt { get; set; }
}
