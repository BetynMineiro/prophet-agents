using Prophet.Domain.Entities.Common;

namespace Prophet.Domain.Entities.Pipeline;

/// <summary>Uploaded input file for a Prophet pipeline project (stored under genesis/prophet/{projectId}/inputs/ in Firebase).</summary>
public class PipelineInputDocument : BaseEntity
{
    public Guid PipelineProjectId { get; set; }

    /// <summary>Original file name as provided by the user (sanitized for display).</summary>
    public string OriginalFileName { get; set; } = string.Empty;

    /// <summary>MIME type (may be application/octet-stream).</summary>
    public string ContentType { get; set; } = "application/octet-stream";

    /// <summary>Full object path in the bucket (return value of storage upload).</summary>
    public string StorageObjectPath { get; set; } = string.Empty;

    public long SizeBytes { get; set; }
}
