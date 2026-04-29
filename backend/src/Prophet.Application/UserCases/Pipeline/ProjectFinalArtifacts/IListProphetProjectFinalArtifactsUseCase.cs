namespace Prophet.Application.UserCases.Pipeline.ProjectFinalArtifacts;

public interface IListPipelineFinalArtifactsUseCase
{
    Task<IReadOnlyList<PipelineFinalArtifactItemDto>?> ExecuteAsync(Guid projectId, CancellationToken cancellationToken = default);
}
