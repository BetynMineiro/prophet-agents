using Prophet.Application.Interfaces.Pipeline;

namespace Prophet.Application.UserCases.Pipeline.PipelineExecution;

public sealed class ListPipelineArtifactsUseCase(IPipelineExecutionStore pipelineStore) : IListPipelineArtifactsUseCase
{
    public async Task<IReadOnlyList<PipelineArtifactItemDto>?> ExecuteAsync(
        Guid projectId,
        Guid versionId,
        CancellationToken cancellationToken = default)
    {
        if (!await pipelineStore.ProjectExistsActiveAsync(projectId, cancellationToken).ConfigureAwait(false))
            return null;

        if (await pipelineStore.GetVersionForProjectAsync(projectId, versionId, cancellationToken).ConfigureAwait(false) == null)
            return null;

        var list = await pipelineStore.ListArtifactsAsync(versionId, cancellationToken).ConfigureAwait(false);
        return list.Select(a => new PipelineArtifactItemDto(a.ArtifactType, a.ContentJson, a.CreatedByAgent, a.CreatedAtUtc)).ToList();
    }
}
