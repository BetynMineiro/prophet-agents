namespace Prophet.Application.UserCases.Pipeline.PipelineExecution;

public interface IGetPipelineArtifactByTypeUseCase
{
    Task<PipelineArtifactItemDto?> ExecuteAsync(Guid projectId, Guid versionId, string artifactType, CancellationToken cancellationToken = default);
}
