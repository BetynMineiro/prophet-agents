namespace Prophet.Application.UserCases.Pipeline.ProjectFinalArtifacts;

public interface IDeletePipelineFinalArtifactUseCase
{
    Task<bool> ExecuteAsync(Guid projectId, Guid documentId, CancellationToken cancellationToken = default);
}
