namespace Prophet.Application.UserCases.Pipeline.PipelineExecution;

/// <summary>Updates <see cref="Prophet.Domain.Entities.Pipeline.ArtifactVersion.CurrentStepIndex"/> for pipeline progress UI.</summary>
public interface ISetPipelineRunStepUseCase
{
    Task ExecuteAsync(Guid projectId, Guid versionId, int stepIndex, CancellationToken cancellationToken = default);
}
