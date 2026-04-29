using System.Text.RegularExpressions;

namespace Prophet.CrossCutting.Validation;

/// <summary>
/// Validates phone numbers before insert/update. Accepts optional + and digits (E.164-style); 10–15 digits.
/// </summary>
public static partial class PhoneNumberValidation
{
    /// <summary>Optional '+' then 10–15 digits (spaces/dashes are not allowed; trim before calling).</summary>
    [GeneratedRegex(@"^\+?[0-9]{10,15}$", RegexOptions.Compiled)]
    private static partial Regex E164LikeRegex();

    /// <summary>
    /// Returns true if <paramref name="value"/> is null/empty/whitespace (optional field) or a valid phone format.
    /// Valid format: optional +, then 10 to 15 digits (e.g. +351912345678 or 912345678).
    /// </summary>
    public static bool IsValid(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return true;
        var trimmed = value.Trim();
        return E164LikeRegex().IsMatch(trimmed);
    }

    /// <summary>
    /// Returns an error message if invalid, or null if valid/empty.
    /// </summary>
    public static string? GetErrorMessage(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;
        var trimmed = value.Trim();
        if (E164LikeRegex().IsMatch(trimmed))
            return null;
        return "Phone number must have 10 to 15 digits, optionally prefixed with + (e.g. +351912345678).";
    }
}
