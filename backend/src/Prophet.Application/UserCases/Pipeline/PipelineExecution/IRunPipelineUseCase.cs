namespace Prophet.Application.UserCases.Pipeline.PipelineExecution;

public interface IRunPipelineUseCase
{
    /// <summary>Starts a full run, or an interactive run (one step then <see cref="PipelineRunStatus.Paused"/>).</summary>
    Task<RunPipelineOutcome> ExecuteAsync(Guid projectId, Guid versionId, RunPipelineRequest? request = null, CancellationToken cancellationToken = default);
}

public record RunPipelineOutcome(
    RunPipelineResponseDto? Data,
    RunPipelineOutcomeKind Kind);

public enum RunPipelineOutcomeKind
{
    Ok = 0,
    ProjectOrVersionNotFound = 1,
    ConflictAlreadyRunningOrCompleted = 2,
    ConflictInvalidPipelineState = 3,
}
