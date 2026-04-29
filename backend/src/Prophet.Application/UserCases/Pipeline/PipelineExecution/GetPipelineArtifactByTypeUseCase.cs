using Prophet.Application.Interfaces.Pipeline;

namespace Prophet.Application.UserCases.Pipeline.PipelineExecution;

public sealed class GetPipelineArtifactByTypeUseCase(IPipelineExecutionStore pipelineStore) : IGetPipelineArtifactByTypeUseCase
{
    public async Task<PipelineArtifactItemDto?> ExecuteAsync(
        Guid projectId,
        Guid versionId,
        string artifactType,
        CancellationToken cancellationToken = default)
    {
        if (!await pipelineStore.ProjectExistsActiveAsync(projectId, cancellationToken).ConfigureAwait(false))
            return null;

        if (await pipelineStore.GetVersionForProjectAsync(projectId, versionId, cancellationToken).ConfigureAwait(false) == null)
            return null;

        var t = artifactType.Trim();
        if (string.IsNullOrEmpty(t))
            return null;

        var a = await pipelineStore.GetArtifactByTypeAsync(versionId, t, cancellationToken).ConfigureAwait(false);
        return a == null
            ? null
            : new PipelineArtifactItemDto(a.ArtifactType, a.ContentJson, a.CreatedByAgent, a.CreatedAtUtc);
    }
}
