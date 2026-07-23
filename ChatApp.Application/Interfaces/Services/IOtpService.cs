namespace ChatApp.Application.Interfaces.Services;

public interface IOtpService
{
    /// <summary>
    /// Tạo và lưu OTP cho số điện thoại (TTL = 3 phút)
    /// </summary>
    /// <returns>Mã OTP đã tạo (để log/test)</returns>
    Task<string> GenerateAndStoreOtpAsync(string phoneNumber);

    /// <summary>
    /// Xác minh OTP cho số điện thoại
    /// </summary>
    /// <returns>true nếu OTP hợp lệ và còn hạn; false nếu sai hoặc hết hạn</returns>
    Task<bool> VerifyOtpAsync(string phoneNumber, string otpCode);
}
