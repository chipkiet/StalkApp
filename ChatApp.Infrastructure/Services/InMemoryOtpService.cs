using ChatApp.Application.Interfaces.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace ChatApp.Infrastructure.Services;

public class InMemoryOtpService : IOtpService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<InMemoryOtpService> _logger;

    // Prefix để tránh conflict key trong cache
    private const string OTP_CACHE_PREFIX = "OTP_";
    // Thời gian sống của OTP
    private static readonly TimeSpan OtpTtl = TimeSpan.FromMinutes(3);

    public InMemoryOtpService(IMemoryCache cache, ILogger<InMemoryOtpService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public Task<string> GenerateAndStoreOtpAsync(string phoneNumber)
    {
        // Tạo mã OTP 6 chữ số
        var otpCode = Random.Shared.Next(100_000, 999_999).ToString();
        var cacheKey = OTP_CACHE_PREFIX + phoneNumber;

        // Lưu vào MemoryCache với TTL
        _cache.Set(cacheKey, otpCode, OtpTtl);

        return Task.FromResult(otpCode);
    }

    public Task<bool> VerifyOtpAsync(string phoneNumber, string otpCode)
    {
        var cacheKey = OTP_CACHE_PREFIX + phoneNumber;

        if (!_cache.TryGetValue(cacheKey, out string? storedOtp))
            return Task.FromResult(false); // OTP không tồn tại hoặc hết hạn

        var isValid = string.Equals(storedOtp, otpCode, StringComparison.Ordinal);

        // Sau khi verify thành công, xoá OTP để không dùng lại được
        if (isValid)
            _cache.Remove(cacheKey);

        return Task.FromResult(isValid);
    }
}
