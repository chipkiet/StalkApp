using Xunit;

namespace ChatApp.WebApi.Tests;

/// <summary>
/// Unit tests for NormalizeVietnamesePhone logic.
/// The helper is extracted here as a static method to allow pure unit testing
/// without spinning up the full WebApplicationFactory.
/// </summary>
public static class PhoneNormalizer
{
    public static string Normalize(string raw)
    {
        var cleaned = raw.Trim()
                         .Replace(" ", "")
                         .Replace("-", "")
                         .Replace(".", "");

        if (cleaned.StartsWith("0084"))
            return "+84" + cleaned[4..];

        if (cleaned.StartsWith("+84"))
            return cleaned;

        if (cleaned.StartsWith("84") && cleaned.Length >= 11)
            return "+84" + cleaned[2..];

        if (cleaned.StartsWith("0"))
            return "+84" + cleaned[1..];

        return cleaned;
    }
}

public class PhoneNormalizationTests
{
    // ── Helper ──────────────────────────────────────────────────────────────
    private static string N(string input) => PhoneNormalizer.Normalize(input);

    // ── Standard Vietnamese formats ─────────────────────────────────────────

    [Theory]
    [InlineData("0123456789",  "+84123456789")]   // 0X -> +84X
    [InlineData("01234567891", "+841234567891")]  // 11-digit old format
    [InlineData("0987654321",  "+84987654321")]   // common 10-digit
    public void Normalize_LeadingZero_ReplacesWithPlusEightyFour(string input, string expected)
    {
        Assert.Equal(expected, N(input));
    }

    [Theory]
    [InlineData("841234567891",  "+841234567891")]   // 84 without +
    [InlineData("84987654321",   "+84987654321")]    // shorter variant
    public void Normalize_EightyFourPrefix_AddsPlusSign(string input, string expected)
    {
        Assert.Equal(expected, N(input));
    }

    [Theory]
    [InlineData("00841234567891", "+841234567891")]  // 0084 international dialing
    [InlineData("008412345678",   "+8412345678")]
    public void Normalize_ZeroZeroEightyFour_StripsTwoLeadingZeros(string input, string expected)
    {
        Assert.Equal(expected, N(input));
    }

    [Theory]
    [InlineData("+841234567891", "+841234567891")] // already E.164
    [InlineData("+84987654321",  "+84987654321")]
    public void Normalize_AlreadyE164_ReturnsSame(string input, string expected)
    {
        Assert.Equal(expected, N(input));
    }

    // ── Whitespace / separator stripping ────────────────────────────────────

    [Theory]
    [InlineData("0123 456 789",  "+84123456789")]  // spaces
    [InlineData("0123-456-789",  "+84123456789")]  // dashes
    [InlineData("0123.456.789",  "+84123456789")]  // dots
    [InlineData(" 0123456789 ", "+84123456789")]   // leading/trailing spaces
    public void Normalize_Separators_AreStripped(string input, string expected)
    {
        Assert.Equal(expected, N(input));
    }

    // ── Idempotent: normalising twice gives same result ─────────────────────

    [Theory]
    [InlineData("0987654321")]
    [InlineData("+84987654321")]
    [InlineData("84987654321")]
    public void Normalize_IsIdempotent(string input)
    {
        var once  = N(input);
        var twice = N(once);
        Assert.Equal(once, twice);
    }
}
