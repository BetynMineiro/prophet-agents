using System.Text;
using System.Text.Json;

namespace Prophet.Application.AgentPipeline;

/// <summary>
/// Builds a bounded excerpt from the file-agent chunks artifact so downstream LLM steps stay grounded in uploaded source text.
/// </summary>
public static class PipelineSourceContextPrompt
{
    /// <summary>Default for market, model, and documentation agents.</summary>
    public const int DefaultMaxChars = 16_000;

    /// <summary>Tighter budget for diagram and POC agents (less noise, same grounding goal).</summary>
    public const int DiagramAndPocMaxChars = 8_000;

    /// <summary>Middle ground for architecture (constraints + structure).</summary>
    public const int ArchitectureMaxChars = 12_000;

    /// <summary>
    /// Parses <paramref name="chunksArtifactJson"/> (file-agent output) and returns a Markdown section with concatenated chunk text, truncated to <paramref name="maxChars"/>.
    /// Truncation is by character count on the joined text (chunks are produced in order by file-agent).
    /// </summary>
    public static string BuildAnchoringSection(string? chunksArtifactJson, int maxChars = DefaultMaxChars)
    {
        if (string.IsNullOrWhiteSpace(chunksArtifactJson) || maxChars <= 0)
            return "";

        try
        {
            using var doc = JsonDocument.Parse(chunksArtifactJson);
            if (!doc.RootElement.TryGetProperty("chunks", out var chunksEl) || chunksEl.ValueKind != JsonValueKind.Array)
                return "";

            var sb = new StringBuilder();
            sb.AppendLine("## Original source material (from uploaded inputs — stay faithful to this; do not invent requirements or names absent below)");
            sb.AppendLine();

            foreach (var el in chunksEl.EnumerateArray())
            {
                if (el.TryGetProperty("text", out var t))
                {
                    var s = t.GetString();
                    if (!string.IsNullOrEmpty(s))
                        sb.AppendLine(s);
                }
            }

            var full = sb.ToString();
            if (full.Length <= maxChars)
                return full;

            return full[..maxChars] + "\n\n[... source truncated for length ...]";
        }
        catch (JsonException)
        {
            return "";
        }
    }

    /// <summary>
    /// Prepends the anchoring section when non-empty; otherwise returns <paramref name="basePrompt"/> unchanged.
    /// </summary>
    public static string PrependSourceAnchoring(string basePrompt, string? chunksArtifactJson, int maxChars = DefaultMaxChars)
    {
        var section = BuildAnchoringSection(chunksArtifactJson, maxChars);
        return string.IsNullOrEmpty(section) ? basePrompt : section + "\n\n" + basePrompt;
    }
}
