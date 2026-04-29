using Prophet.Application.Interfaces.Pipeline;
using Prophet.Domain.Entities.Pipeline;

namespace Prophet.Application.UserCases.Pipeline.PipelineExecution;

public sealed class RewindPipelineToStepUseCase(
    IPipelineExecutionStore pipelineStore,
    ClearPipelineOutputsFromStepUseCase clearPipelineOutputsFromStep) : IRewindPipelineToStepUseCase
{
    public async Task<RunPipelineOutcome> ExecuteAsync(
        Guid projectId,
        Guid versionId,
        RewindPipelineToStepRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!await pipelineStore.ProjectExistsActiveAsync(projectId, cancellationToken).ConfigureAwait(false))
            return new RunPipelineOutcome(null, RunPipelineOutcomeKind.ProjectOrVersionNotFound);

        var v = await pipelineStore.GetVersionForUpdateAsync(projectId, versionId, cancellationToken).ConfigureAwait(false);
        if (v == null)
            return new RunPipelineOutcome(null, RunPipelineOutcomeKind.ProjectOrVersionNotFound);

        if (v.PipelineStatus is not (
                PipelineRunStatus.Paused
                or PipelineRunStatus.Failed
                or PipelineRunStatus.Completed))
            return new RunPipelineOutcome(null, RunPipelineOutcomeKind.ConflictInvalidPipelineState);

        var target = request.TargetStepIndex;
        if (target < 0 || target >= MainPipelineStepIds.TotalSteps)
            return new RunPipelineOutcome(null, RunPipelineOutcomeKind.ConflictInvalidPipelineState);

        await clearPipelineOutputsFromStep.ExecuteAsync(v.Id, target, cancellationToken).ConfigureAwait(false);

        v.CurrentStepIndex = target;
        v.PipelineStatus = PipelineRunStatus.Paused;
        v.PipelineError = null;
        v.PipelineCompletedAtUtc = null;
        await pipelineStore.PersistChangesAsync(cancellationToken).ConfigureAwait(false);

        var dto = new RunPipelineResponseDto(
            v.Id,
            PipelineExecutionStatusHelper.StatusString(v.PipelineStatus),
            v.CurrentStepIndex,
            MainPipelineStepIds.TotalSteps);
        return new RunPipelineOutcome(dto, RunPipelineOutcomeKind.Ok);
    }
}
