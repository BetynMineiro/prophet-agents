using Prophet.Application.UserCases.Pipeline.PipelineExecution;
using Prophet.Domain.Entities.Pipeline;

namespace Prophet.Application.UserCases.Pipeline.Projects;

/// <param name="IsActive">False when the project is soft-deleted (<c>DeletedAtUtc</c> set).</param>
/// <param name="ExpectedDate">Planned date (date-only); null if not set.</param>
/// <param name="LatestArtifactVersionId">Highest <see cref="ArtifactVersion.VersionNumber"/> for the project; null if none.</param>
/// <param name="LatestPipelineStatus">Same strings as pipeline status API (<see cref="PipelineExecutionStatusHelper.StatusString"/>); null if no versions.</param>
public record PipelineProjectItemDto(
    Guid Id,
    string Name,
    string? Description,
    DateOnly? ExpectedDate,
    DateTime CreatedAtUtc,
    bool IsActive,
    Guid? LatestArtifactVersionId,
    string? LatestPipelineStatus)
{
    public static PipelineProjectItemDto FromProject(
        PipelineProject p,
        (Guid VersionId, PipelineRunStatus PipelineStatus)? latest) =>
        new(
            p.Id,
            p.Name,
            p.Description,
            p.ExpectedDate,
            p.CreatedAtUtc,
            p.DeletedAtUtc == null,
            latest?.VersionId,
            latest.HasValue
                ? PipelineExecutionStatusHelper.StatusString(latest.Value.PipelineStatus)
                : null);
}
