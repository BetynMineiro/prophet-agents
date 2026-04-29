namespace Prophet.CrossCutting.Extensions;

public static class SearchTextExtensions
{
    public static string? NormalizeSearchText(this string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;
        if (maxLength <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxLength), "maxLength must be greater than zero.");

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
            trimmed = trimmed[..maxLength];

        return trimmed.ToLowerInvariant();
    }
}
