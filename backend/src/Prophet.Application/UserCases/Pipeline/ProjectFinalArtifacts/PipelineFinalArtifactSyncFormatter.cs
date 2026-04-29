using System.Text;
using System.Text.Json;

namespace Prophet.Application.UserCases.Pipeline.ProjectFinalArtifacts;

/// <summary>Builds Markdown files for final-artifacts from pipeline JSON / text.</summary>
public static class PipelineFinalArtifactSyncFormatter
{
    private static readonly JsonSerializerOptions PrettyJson = new() { WriteIndented = true };

    public static string BuildArchitectureMarkdown(string versionLabel, string json)
    {
        var pretty = PrettyPrintJson(json);
        var sb = new StringBuilder();
        sb.AppendLine("# Architecture");
        sb.AppendLine();
        sb.AppendLine($"*Generated from pipeline {versionLabel}*");
        sb.AppendLine();
        sb.AppendLine("```json");
        sb.AppendLine(pretty);
        sb.AppendLine("```");
        return sb.ToString();
    }

    /// <summary>Prefers a ```mermaid fence when JSON has a top-level "mermaid" string; otherwise embeds pretty JSON.</summary>
    public static string BuildDiagramMarkdown(string title, string versionLabel, string artifactJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(artifactJson) ? "{}" : artifactJson);
            if (doc.RootElement.TryGetProperty("mermaid", out var m)
                && m.ValueKind == JsonValueKind.String)
            {
                var src = m.GetString() ?? "";
                var sb = new StringBuilder();
                sb.AppendLine($"# {title}");
                sb.AppendLine();
                sb.AppendLine($"*Generated from pipeline {versionLabel}*");
                sb.AppendLine();
                sb.AppendLine("```mermaid");
                sb.AppendLine(src);
                sb.AppendLine("```");
                return sb.ToString();
            }
        }
        catch (JsonException)
        {
            /* fall through */
        }

        return BuildJsonFallbackMarkdown(title, versionLabel, artifactJson);
    }

    public static string BuildJsonFallbackMarkdown(string title, string versionLabel, string rawJson)
    {
        var pretty = PrettyPrintJson(rawJson);
        var sb = new StringBuilder();
        sb.AppendLine($"# {title}");
        sb.AppendLine();
        sb.AppendLine($"*Generated from pipeline {versionLabel}*");
        sb.AppendLine();
        sb.AppendLine("```json");
        sb.AppendLine(pretty);
        sb.AppendLine("```");
        return sb.ToString();
    }

    public static string PrettyPrintJson(string raw)
    {
        try
        {
            using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(raw) ? "{}" : raw);
            return JsonSerializer.Serialize(doc.RootElement, PrettyJson);
        }
        catch (JsonException)
        {
            return raw;
        }
    }
}
