using Prophet.Application.Interfaces.Pipeline;

namespace Prophet.Application.UserCases.Pipeline.PipelineExecution;

public sealed class GetArtifactVersionUseCase(IPipelineExecutionStore pipelineStore) : IGetArtifactVersionUseCase
{
    public async Task<ArtifactVersionItemDto?> ExecuteAsync(
        Guid projectId,
        Guid versionId,
        CancellationToken cancellationToken = default)
    {
        if (!await pipelineStore.ProjectExistsActiveAsync(projectId, cancellationToken).ConfigureAwait(false))
            return null;

        var v = await pipelineStore.GetVersionForProjectAsync(projectId, versionId, cancellationToken).ConfigureAwait(false);
        return v == null ? null : PipelineExecutionStatusHelper.ToItemDto(v);
    }
}
