using Microsoft.Extensions.Logging;
using Prophet.Application.AgentPipeline;
using Prophet.Application.Interfaces.Pipeline;
using Prophet.Application.UserCases.Pipeline.ProjectFinalArtifacts;
using Prophet.Domain.Entities.Pipeline;

namespace Prophet.Application.UserCases.Pipeline.PipelineExecution;

public sealed class RetryPipelineStepUseCase(
    IPipelineExecutionStore pipelineStore,
    IPipelineAgentExecutor pipelineAgentExecutor,
    ClearPipelineOutputsFromStepUseCase clearPipelineOutputsFromStep,
    ISyncPipelineGeneratedFinalArtifactsUseCase syncPipelineGeneratedFinalArtifacts,
    ILogger<RetryPipelineStepUseCase> logger) : IRetryPipelineStepUseCase
{
    public async Task<RunPipelineOutcome> ExecuteAsync(
        Guid projectId,
        Guid versionId,
        RetryPipelineStepRequest request,
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

        var stepIndex = request.StepIndex;
        if (stepIndex < 0 || stepIndex >= MainPipelineStepIds.TotalSteps)
            return new RunPipelineOutcome(null, RunPipelineOutcomeKind.ConflictInvalidPipelineState);

        await clearPipelineOutputsFromStep.ExecuteAsync(v.Id, stepIndex, cancellationToken).ConfigureAwait(false);

        v.CurrentStepIndex = stepIndex;
        v.PipelineStatus = PipelineRunStatus.Running;
        v.PipelineError = null;
        await pipelineStore.PersistChangesAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            await pipelineAgentExecutor.ExecuteSingleStepAsync(projectId, v, stepIndex, cancellationToken).ConfigureAwait(false);
            if (v.CurrentStepIndex >= MainPipelineStepIds.TotalSteps)
            {
                v.PipelineStatus = PipelineRunStatus.Completed;
                v.PipelineCompletedAtUtc = DateTime.UtcNow;
            }
            else
            {
                v.PipelineStatus = PipelineRunStatus.Paused;
            }
        }
        catch (Exception ex)
        {
            v.PipelineStatus = PipelineRunStatus.Failed;
            v.PipelineError = ex.Message;
            v.PipelineCompletedAtUtc = null;
        }

        await pipelineStore.PersistChangesAsync(cancellationToken).ConfigureAwait(false);

        if (await pipelineStore.ReconcileRunningToCompletedWhenAllStepsDoneAsync(projectId, versionId, cancellationToken).ConfigureAwait(false))
        {
            v.PipelineStatus = PipelineRunStatus.Completed;
            v.PipelineCompletedAtUtc = DateTime.UtcNow;
        }

        if (v.PipelineStatus == PipelineRunStatus.Completed)
        {
            try
            {
                await syncPipelineGeneratedFinalArtifacts.ExecuteAsync(projectId, versionId, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.LogWarning(
                    ex,
                    "Sync of pipeline-generated final artifacts failed for project {ProjectId} version {VersionId}",
                    projectId,
                    versionId);
            }
        }

        var dto = new RunPipelineResponseDto(
            v.Id,
            PipelineExecutionStatusHelper.StatusString(v.PipelineStatus),
            v.CurrentStepIndex,
            MainPipelineStepIds.TotalSteps);
        return new RunPipelineOutcome(dto, RunPipelineOutcomeKind.Ok);
    }
}
