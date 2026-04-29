namespace Prophet.CrossCutting.Validation;

/// <summary>
/// Validates optional avatar URL (http/https, absolute, max length 2048).
/// </summary>
public static class AvatarUrlValidation
{
    public const int MaxLength = 2048;

    /// <summary>
    /// Returns true if <paramref name="value"/> is null/empty/whitespace (optional) or a valid absolute http/https URL.
    /// </summary>
    public static bool IsValid(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return true;
        var trimmed = value.Trim();
        if (trimmed.Length > MaxLength)
            return false;
        return Uri.TryCreate(trimmed, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    /// <summary>
    /// Returns an error message if invalid, or null if valid/empty.
    /// </summary>
    public static string? GetErrorMessage(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;
        var trimmed = value.Trim();
        if (trimmed.Length > MaxLength)
            return $"AvatarUrl must be at most {MaxLength} characters.";
        if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri) || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            return "AvatarUrl must be a valid http or https URL.";
        return null;
    }
}
