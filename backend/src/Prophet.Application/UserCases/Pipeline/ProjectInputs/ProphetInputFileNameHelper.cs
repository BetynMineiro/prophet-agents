namespace Prophet.Application.UserCases.Pipeline.ProjectInputs;

public static class InputFileNameHelper
{
    private const int MaxOriginalLength = 400;

    public static string SanitizeOriginalFileName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "file";
        var n = Path.GetFileName(name.Trim());
        foreach (var c in Path.GetInvalidFileNameChars())
            n = n.Replace(c, '_');
        if (string.IsNullOrWhiteSpace(n))
            return "file";
        return n.Length <= MaxOriginalLength ? n : n[..MaxOriginalLength];
    }
}
