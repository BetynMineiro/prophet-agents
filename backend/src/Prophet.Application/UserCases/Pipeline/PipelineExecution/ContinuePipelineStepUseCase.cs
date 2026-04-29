using Microsoft.Extensions.Logging;
using Prophet.Application.AgentPipeline;
using Prophet.Application.Interfaces.Pipeline;
using Prophet.Application.UserCases.Pipeline.ProjectFinalArtifacts;
using Prophet.Domain.Entities.Pipeline;

namespace Prophet.Application.UserCases.Pipeline.PipelineExecution;

public sealed class ContinuePipelineStepUseCase(
    IPipelineExecutionStore pipelineStore,
    IPipelineAgentExecutor pipelineAgentExecutor,
    ISyncPipelineGeneratedFinalArtifactsUseCase syncPipelineGeneratedFinalArtifacts,
    ILogger<ContinuePipelineStepUseCase> logger) : IContinuePipelineStepUseCase
{
    public async Task<RunPipelineOutcome> ExecuteAsync(
        Guid projectId,
        Guid versionId,
        CancellationToken cancellationToken = default)
    {
        if (!await pipelineStore.ProjectExistsActiveAsync(projectId, cancellationToken).ConfigureAwait(false))
            return new RunPipelineOutcome(null, RunPipelineOutcomeKind.ProjectOrVersionNotFound);

        var v = await pipelineStore.GetVersionForUpdateAsync(projectId, versionId, cancellationToken).ConfigureAwait(false);
        if (v == null)
            return new RunPipelineOutcome(null, RunPipelineOutcomeKind.ProjectOrVersionNotFound);

        if (v.PipelineStatus != PipelineRunStatus.Paused)
            return new RunPipelineOutcome(null, RunPipelineOutcomeKind.ConflictInvalidPipelineState);

        if (v.CurrentStepIndex >= MainPipelineStepIds.TotalSteps)
            return new RunPipelineOutcome(null, RunPipelineOutcomeKind.ConflictInvalidPipelineState);

        v.PipelineStatus = PipelineRunStatus.Running;
        v.PipelineError = null;
        await pipelineStore.PersistChangesAsync(cancellationToken).ConfigureAwait(false);

        var next = v.CurrentStepIndex;
        try
        {
            await pipelineAgentExecutor.ExecuteSingleStepAsync(projectId, v, next, cancellationToken).ConfigureAwait(false);
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
