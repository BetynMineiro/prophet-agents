using Microsoft.Extensions.Logging;
using Prophet.Application.AgentPipeline;
using Prophet.Application.Interfaces.Pipeline;
using Prophet.Application.UserCases.Pipeline.ProjectFinalArtifacts;
using Prophet.Domain.Entities.Pipeline;

namespace Prophet.Application.UserCases.Pipeline.PipelineExecution;

public sealed class RunPipelineUseCase(
    IPipelineExecutionStore pipelineStore,
    IPipelineAgentExecutor pipelineAgentExecutor,
    ClearPipelineOutputsFromStepUseCase clearOutputsFromStep,
    ISyncPipelineGeneratedFinalArtifactsUseCase syncPipelineGeneratedFinalArtifacts,
    ILogger<RunPipelineUseCase> logger) : IRunPipelineUseCase
{
    public async Task<RunPipelineOutcome> ExecuteAsync(
        Guid projectId,
        Guid versionId,
        RunPipelineRequest? request = null,
        CancellationToken cancellationToken = default)
    {
        if (!await pipelineStore.ProjectExistsActiveAsync(projectId, cancellationToken).ConfigureAwait(false))
            return new RunPipelineOutcome(null, RunPipelineOutcomeKind.ProjectOrVersionNotFound);

        var v = await pipelineStore.GetVersionForUpdateAsync(projectId, versionId, cancellationToken).ConfigureAwait(false);
        if (v == null)
            return new RunPipelineOutcome(null, RunPipelineOutcomeKind.ProjectOrVersionNotFound);

        var interactive = request?.InteractiveSteps ?? false;

        // Only one execution at a time; Paused uses Continue, not Run.
        if (v.PipelineStatus == PipelineRunStatus.Running)
            return new RunPipelineOutcome(null, RunPipelineOutcomeKind.ConflictAlreadyRunningOrCompleted);

        if (v.PipelineStatus == PipelineRunStatus.Paused)
            return new RunPipelineOutcome(null, RunPipelineOutcomeKind.ConflictInvalidPipelineState);

        // Idle, Failed, or Completed: Run starts over from step 0 (interactive: first step only).

        v.PipelineError = null;
        v.PipelineCompletedAtUtc = null;
        var utc = DateTime.UtcNow;

        try
        {
            if (interactive)
            {
                v.PipelineStartedAtUtc = utc;
                await clearOutputsFromStep.ExecuteAsync(v.Id, 0, cancellationToken).ConfigureAwait(false);
                v.PipelineStatus = PipelineRunStatus.Running;
                v.CurrentStepIndex = 0;
                await pipelineStore.PersistChangesAsync(cancellationToken).ConfigureAwait(false);

                await pipelineAgentExecutor.ExecuteSingleStepAsync(projectId, v, 0, cancellationToken).ConfigureAwait(false);
                ApplyStatusAfterInteractiveStep(v);
            }
            else
            {
                v.PipelineStartedAtUtc = utc;
                var refinementResume =
                    v.PipelineStatus == PipelineRunStatus.Idle
                    && v.ParentVersionId.HasValue
                    && v.CurrentStepIndex > 0
                    && v.CurrentStepIndex < MainPipelineStepIds.TotalSteps;

                v.PipelineStatus = PipelineRunStatus.Running;
                var startStep = refinementResume ? v.CurrentStepIndex : 0;
                if (!refinementResume)
                    v.CurrentStepIndex = 0;

                if (!refinementResume)
                    await clearOutputsFromStep.ExecuteAsync(v.Id, 0, cancellationToken).ConfigureAwait(false);

                await pipelineStore.PersistChangesAsync(cancellationToken).ConfigureAwait(false);

                if (refinementResume)
                    await pipelineAgentExecutor.ExecuteFromStepInclusiveAsync(projectId, v, startStep, cancellationToken).ConfigureAwait(false);
                else
                    await pipelineAgentExecutor.ExecuteAsync(projectId, v, cancellationToken).ConfigureAwait(false);

                v.PipelineStatus = PipelineRunStatus.Completed;
                v.PipelineCompletedAtUtc = DateTime.UtcNow;
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

    private static void ApplyStatusAfterInteractiveStep(ArtifactVersion v)
    {
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
}
