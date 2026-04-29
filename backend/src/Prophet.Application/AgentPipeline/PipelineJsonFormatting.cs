using System.Text.Json;

namespace Prophet.Application.AgentPipeline;

internal static class PipelineJsonFormatting
{
    /// <summary>Strips optional ```json ... ``` fences from LLM output.</summary>
    internal static string StripCodeFences(string text)
    {
        var s = text.Trim();
        if (!s.StartsWith("```", StringComparison.Ordinal))
            return s;

        var lines = s.Split('\n');
        if (lines.Length < 2)
            return s;

        var sb = new System.Text.StringBuilder();
        for (var i = 1; i < lines.Length; i++)
        {
            var line = lines[i];
            if (line.Trim().Equals("```", StringComparison.Ordinal))
                break;
            sb.Append(line);
            if (i < lines.Length - 1)
                sb.Append('\n');
        }

        return sb.ToString().Trim();
    }

    /// <summary>Ensures stored JSON is valid; wraps raw text if parsing fails.</summary>
    internal static string NormalizeJsonObject(string text)
    {
        var trimmed = StripCodeFences(text).Trim();
        try
        {
            using var _ = JsonDocument.Parse(trimmed);
            return trimmed;
        }
        catch (JsonException)
        {
            return JsonSerializer.Serialize(new { raw = trimmed });
        }
    }
}
