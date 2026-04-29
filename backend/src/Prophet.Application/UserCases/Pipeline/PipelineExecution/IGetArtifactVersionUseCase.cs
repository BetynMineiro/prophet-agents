namespace Prophet.Application.UserCases.Pipeline.PipelineExecution;

public interface IGetArtifactVersionUseCase
{
    Task<ArtifactVersionItemDto?> ExecuteAsync(Guid projectId, Guid versionId, CancellationToken cancellationToken = default);
}
