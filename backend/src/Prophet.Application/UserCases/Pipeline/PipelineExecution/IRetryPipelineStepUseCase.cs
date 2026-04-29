namespace Prophet.Application.UserCases.Pipeline.PipelineExecution;

/// <summary>Clears outputs from <paramref name="request.StepIndex"/> onward and re-executes that step.</summary>
public interface IRetryPipelineStepUseCase
{
    Task<RunPipelineOutcome> ExecuteAsync(Guid projectId, Guid versionId, RetryPipelineStepRequest request, CancellationToken cancellationToken = default);
}
