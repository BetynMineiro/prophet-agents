namespace Prophet.Application.UserCases.Pipeline.PipelineExecution;

/// <summary>Deletes outputs from <paramref name="request.TargetStepIndex"/> onward and sets the next step to run (paused).</summary>
public interface IRewindPipelineToStepUseCase
{
    Task<RunPipelineOutcome> ExecuteAsync(Guid projectId, Guid versionId, RewindPipelineToStepRequest request, CancellationToken cancellationToken = default);
}
