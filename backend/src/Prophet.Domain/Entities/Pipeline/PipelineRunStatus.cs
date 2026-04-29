namespace Prophet.Domain.Entities.Pipeline;

/// <summary>Pipeline execution state for an <see cref="ArtifactVersion"/> (§7 status).</summary>
public enum PipelineRunStatus
{
    Idle = 0,
    Running = 1,
    Completed = 2,
    Failed = 3,

    /// <summary>Interactive mode: a step finished; waiting for continue / retry / rewind.</summary>
    Paused = 4,
}
