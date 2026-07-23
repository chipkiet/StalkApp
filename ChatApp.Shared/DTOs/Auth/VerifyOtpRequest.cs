namespace ChatApp.Shared.DTOs.Auth;

public class VerifyOtpRequest
{
    /// <summary>
    /// Số điện thoại đã nhận OTP
    /// </summary>
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// Mã OTP 6 chữ số nhận được
    /// </summary>
    public string OtpCode { get; set; } = string.Empty;
}
