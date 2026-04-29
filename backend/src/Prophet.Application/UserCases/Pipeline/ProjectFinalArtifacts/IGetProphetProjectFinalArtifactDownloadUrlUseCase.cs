namespace Prophet.Application.UserCases.Pipeline.ProjectFinalArtifacts;

public interface IGetPipelineFinalArtifactDownloadUrlUseCase
{
    Task<PipelineFinalArtifactDownloadDto?> ExecuteAsync(
        Guid projectId,
        Guid documentId,
        CancellationToken cancellationToken = default);
}
