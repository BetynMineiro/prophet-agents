using System.Text;
using Prophet.Application.AgentPipeline;
using Prophet.Application.Interfaces.Llm;
using Prophet.Domain.Entities.Pipeline;

namespace Prophet.Application.AgentPipeline.Agents;

/// <summary>doc-agent → documentation.md in storage (§4 #9).</summary>
public sealed class DocAgent : IPipelineAgent
{
    public string StepId => MainPipelineStepIds.StepIds[8];

    public async Task RunAsync(PipelineAgentRunContext context, CancellationToken cancellationToken = default)
    {
        var chunks = await context.Store.GetArtifactByTypeAsync(context.VersionId, ArtifactTypeNames.Chunks, cancellationToken).ConfigureAwait(false);
        var artifacts = await context.Store.ListArtifactsAsync(context.VersionId, cancellationToken).ConfigureAwait(false);
        if (artifacts.Count == 0)
            throw new InvalidOperationException("No pipeline artifacts to document.");

        var sourceAnchor = PipelineSourceContextPrompt.BuildAnchoringSection(chunks?.ContentJson);

        var sb = new StringBuilder();
        if (!string.IsNullOrEmpty(sourceAnchor))
        {
            sb.AppendLine(sourceAnchor);
            sb.AppendLine();
        }

        sb.AppendLine("Artifacts summary (JSON excerpts truncated):");
        foreach (var a in artifacts.OrderBy(x => x.ArtifactType))
        {
            var excerpt = a.ContentJson.Length > 4000 ? a.ContentJson[..4000] + "…" : a.ContentJson;
            sb.AppendLine();
            sb.AppendLine($"## {a.ArtifactType}");
            sb.AppendLine(excerpt);
        }

        var prompt = """
            You are the doc-agent. Write project documentation as GitHub-flavored Markdown only (no JSON wrapper).
            Include: overview, domain summary, architecture summary, diagrams reference, and next steps.
            Prefer facts grounded in the original source material when present above; treat truncated JSON excerpts as secondary summaries.
            Source material:

            """ + sb;

        var response = await context.CompleteAsync(LlmCategories.Structured, prompt, temperature: 0.3, maxCompletionTokens: 8192, cancellationToken).ConfigureAwait(false);
        var md = response.Content.Trim();
        if (md.StartsWith("```", StringComparison.Ordinal))
            md = PipelineJsonFormatting.StripCodeFences(md);

        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(md));
        var uploaded = await context.UploadVersionFileAsync(
            "documentation",
            ArtifactFileTypeNames.Documentation,
            stream,
            "documentation.md",
            "text/markdown",
            cancellationToken).ConfigureAwait(false);
        if (uploaded == null)
            throw new InvalidOperationException("Upload failed for documentation.");
    }
}
