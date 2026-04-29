namespace Prophet.Application.UserCases.Pipeline.PipelineExecution;

public interface ICreateArtifactVersionUseCase
{
    Task<ArtifactVersionItemDto?> ExecuteAsync(Guid projectId, CreateArtifactVersionRequest request, CancellationToken cancellationToken = default);
}
