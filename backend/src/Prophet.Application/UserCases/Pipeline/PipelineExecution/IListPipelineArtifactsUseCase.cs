namespace Prophet.Application.UserCases.Pipeline.PipelineExecution;

public interface IListPipelineArtifactsUseCase
{
    Task<IReadOnlyList<PipelineArtifactItemDto>?> ExecuteAsync(Guid projectId, Guid versionId, CancellationToken cancellationToken = default);
}
