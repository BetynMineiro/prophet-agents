using Prophet.Domain.Entities.Common;

namespace Prophet.Domain.Entities.Pipeline;

/// <summary>One pipeline run / refinement generation (maps to ARTIFACT_VERSIONS in §5).</summary>
public class ArtifactVersion : BaseEntity
{
    public Guid PipelineProjectId { get; set; }

    /// <summary>Monotonic per project (1, 2, 3…).</summary>
    public int VersionNumber { get; set; }

    public Guid? ParentVersionId { get; set; }

    public string? ChangeSummary { get; set; }

    public PipelineRunStatus PipelineStatus { get; set; }

    /// <summary>0-based index into <see cref="MainPipelineStepIds.StepIds"/> while running; after completion equals step count.</summary>
    public int CurrentStepIndex { get; set; }

    public string? PipelineError { get; set; }

    public DateTime? PipelineStartedAtUtc { get; set; }

    public DateTime? PipelineCompletedAtUtc { get; set; }
}
