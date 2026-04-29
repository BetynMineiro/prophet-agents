using Prophet.Domain.Entities.Common;

namespace Prophet.Domain.Entities.Pipeline;

/// <summary>Single-slot HTML POC per <see cref="HtmlPocKind"/> (stored under genesis/prophet/{projectId}/html-pocs/).</summary>
public class PipelineHtmlPoc : BaseEntity
{
    public Guid PipelineProjectId { get; set; }

    public HtmlPocKind PocKind { get; set; }

    public string OriginalFileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = "text/html";

    public string StorageObjectPath { get; set; } = string.Empty;

    public long SizeBytes { get; set; }
}
