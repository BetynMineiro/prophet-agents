namespace Prophet.Application.UserCases.Pipeline.PipelineExecution;

/// <summary>Persists a JSONB pipeline artifact row (used by agents and orchestration).</summary>
public interface IAddPipelineArtifactUseCase
{
    Task ExecuteAsync(
        Guid versionId,
        string artifactType,
        string contentJson,
        string createdByAgent,
        CancellationToken cancellationToken = default);
}
