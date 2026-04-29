namespace Prophet.Application.UserCases.Pipeline.ProjectFinalArtifacts;

public interface IGetPipelineFinalArtifactContentUseCase
{
    Task<PipelineFinalArtifactContentDto?> ExecuteAsync(
        Guid projectId,
        Guid documentId,
        CancellationToken cancellationToken = default);
}
