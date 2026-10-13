using System.Text.RegularExpressions;

namespace TakiWebApi.Services;

// Telefon numaraları veritabanında farklı biçimlerde kayıtlı ("05551234567",
// "+905551234567", "5551234567"). Yeni kayıtlar tek biçimde saklanır, girişte
// ise olası tüm yazımlar denenir.
public static class PhoneNumbers
{
    // "+90" + 10 hane biçimine getirir.
    public static string Normalize(string? input)
    {
        var digits = Regex.Replace(input ?? "", @"\D", "");
        if (digits.StartsWith("00")) digits = digits[2..];
        if (digits.StartsWith("90") && digits.Length == 12) digits = digits[2..];
        if (digits.StartsWith("0")) digits = digits[1..];
        return "+90" + digits;
    }

    public static IEnumerable<string> Candidates(string? input)
    {
        var raw = (input ?? "").Trim();
        var normalized = Normalize(raw);
        var tenDigits = normalized[3..];
        return new[] { raw, normalized, tenDigits, "0" + tenDigits, "90" + tenDigits }
            .Where(p => p.Length > 0)
            .Distinct();
    }
}
