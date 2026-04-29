using System.Text.Json;
using Prophet.Domain.Entities.Pipeline;

namespace Prophet.Application.AgentPipeline.Agents;

/// <summary>packaging-agent → packaging-manifest JSON (no LLM, no archives).</summary>
public sealed class PackagingAgent : IPipelineAgent
{
    public string StepId => MainPipelineStepIds.StepIds[9];

    public async Task RunAsync(PipelineAgentRunContext context, CancellationToken cancellationToken = default)
    {
        var artifacts = await context.Store.ListArtifactsAsync(context.VersionId, cancellationToken).ConfigureAwait(false);
        var files = await context.Store.ListFilesAsync(context.VersionId, cancellationToken).ConfigureAwait(false);

        var payload = new
        {
            generatedAtUtc = DateTime.UtcNow,
            artifacts = artifacts
                .OrderBy(a => a.ArtifactType)
                .Select(a => new { a.ArtifactType, a.CreatedByAgent, a.CreatedAtUtc })
                .ToList(),
            files = files
                .OrderBy(f => f.FileType)
                .ThenBy(f => f.OriginalFileName)
                .Select(f => new { f.FileType, f.OriginalFileName, f.StorageObjectPath })
                .ToList(),
        };

        var json = JsonSerializer.Serialize(payload, PipelineJsonDefaults.SerializerOptions);
        await context.SaveArtifactJsonAsync(ArtifactTypeNames.PackagingManifest, "packaging-agent", json, cancellationToken).ConfigureAwait(false);
    }
}
