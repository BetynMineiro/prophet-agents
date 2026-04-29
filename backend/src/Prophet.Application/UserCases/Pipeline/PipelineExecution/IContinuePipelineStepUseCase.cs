namespace Prophet.Application.UserCases.Pipeline.PipelineExecution;

/// <summary>Runs the next pipeline step when status is <see cref="Prophet.Domain.Entities.Pipeline.PipelineRunStatus.Paused"/>.</summary>
public interface IContinuePipelineStepUseCase
{
    Task<RunPipelineOutcome> ExecuteAsync(Guid projectId, Guid versionId, CancellationToken cancellationToken = default);
}
