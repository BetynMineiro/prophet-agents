namespace Prophet.Domain.Entities.Pipeline;

/// <summary>Stored file URL / object path for a version (maps to FILES in §5).</summary>
public class PipelineVersionFile
{
    public Guid Id { get; set; }

    public Guid VersionId { get; set; }

    public string FileType { get; set; } = string.Empty;

    /// <summary>GCS/Firebase object path; signed URLs generated on read.</summary>
    public string StorageObjectPath { get; set; } = string.Empty;

    public string? OriginalFileName { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
