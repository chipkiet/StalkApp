namespace ChatApp.Shared.DTOs.Auth;

public class SendOtpRequest
{
    /// <summary>
    /// Số điện thoại cần nhận OTP (định dạng quốc tế, ví dụ: +84912345678)
    /// </summary>
    public string PhoneNumber { get; set; } = string.Empty;
}
