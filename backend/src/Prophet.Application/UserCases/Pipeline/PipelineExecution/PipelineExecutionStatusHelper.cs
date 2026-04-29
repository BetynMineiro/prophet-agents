using Prophet.Domain.Entities.Pipeline;

namespace Prophet.Application.UserCases.Pipeline.PipelineExecution;

internal static class PipelineExecutionStatusHelper
{
    public static string StatusString(PipelineRunStatus s) => s switch
    {
        PipelineRunStatus.Idle => "Idle",
        PipelineRunStatus.Running => "Running",
        PipelineRunStatus.Completed => "Completed",
        PipelineRunStatus.Failed => "Failed",
        PipelineRunStatus.Paused => "Paused",
        _ => "Unknown"
    };

    public static IReadOnlyList<PipelineStepStatusItemDto> BuildSteps(ArtifactVersion v)
    {
        var ids = MainPipelineStepIds.StepIds;
        var list = new List<PipelineStepStatusItemDto>(ids.Count);
        for (var i = 0; i < ids.Count; i++)
        {
            var status = StepStatus(v, i);
            list.Add(new PipelineStepStatusItemDto(ids[i], status));
        }

        return list;
    }

    private static string StepStatus(ArtifactVersion v, int stepIndex)
    {
        return v.PipelineStatus switch
        {
            PipelineRunStatus.Completed => "completed",
            PipelineRunStatus.Idle => "pending",
            PipelineRunStatus.Failed => stepIndex < v.CurrentStepIndex ? "completed" : "failed",
            PipelineRunStatus.Running => stepIndex < v.CurrentStepIndex
                ? "completed"
                : stepIndex == v.CurrentStepIndex ? "running" : "pending",
            PipelineRunStatus.Paused => stepIndex < v.CurrentStepIndex
                ? "completed"
                : stepIndex == v.CurrentStepIndex ? "waiting" : "pending",
            _ => "pending"
        };
    }

    public static ArtifactVersionItemDto ToItemDto(ArtifactVersion v) =>
        new(
            v.Id,
            v.PipelineProjectId,
            v.VersionNumber,
            v.ParentVersionId,
            v.ChangeSummary,
            StatusString(v.PipelineStatus),
            v.CurrentStepIndex,
            MainPipelineStepIds.TotalSteps,
            v.CreatedAtUtc);

    public static PipelineRunStatusResponseDto ToStatusDto(ArtifactVersion v) =>
        new(
            v.Id,
            StatusString(v.PipelineStatus),
            v.CurrentStepIndex,
            MainPipelineStepIds.TotalSteps,
            BuildSteps(v),
            v.PipelineError,
            v.PipelineStartedAtUtc,
            v.PipelineCompletedAtUtc);
}
