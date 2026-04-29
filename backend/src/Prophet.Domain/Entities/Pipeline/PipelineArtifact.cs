namespace Prophet.Domain.Entities.Pipeline;

/// <summary>JSONB artifact row (maps to ARTIFACTS in §5).</summary>
public class PipelineArtifact
{
    public Guid Id { get; set; }

    public Guid VersionId { get; set; }

    public string ArtifactType { get; set; } = string.Empty;

    /// <summary>JSON payload (PostgreSQL jsonb).</summary>
    public string ContentJson { get; set; } = "{}";

    public string? CreatedByAgent { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
