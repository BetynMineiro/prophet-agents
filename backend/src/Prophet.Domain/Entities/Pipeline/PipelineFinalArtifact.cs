using Prophet.Domain.Entities.Common;

namespace Prophet.Domain.Entities.Pipeline;

/// <summary>Generated final artifact (.md) for a Prophet pipeline project (stored under genesis/prophet/{projectId}/final-artifacts/ in Firebase).</summary>
public class PipelineFinalArtifact : BaseEntity
{
    public Guid PipelineProjectId { get; set; }

    /// <summary>Original file name as provided by the user (sanitized for display).</summary>
    public string OriginalFileName { get; set; } = string.Empty;

    /// <summary>MIME type (typically text/markdown).</summary>
    public string ContentType { get; set; } = "text/markdown";

    /// <summary>Full object path in the bucket (return value of storage upload).</summary>
    public string StorageObjectPath { get; set; } = string.Empty;

    public long SizeBytes { get; set; }
}
