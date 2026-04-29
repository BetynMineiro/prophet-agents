using Prophet.Domain.Entities.Common;

namespace Prophet.Domain.Entities.Pipeline;

/// <summary>Document pipeline project scope. Soft-deleted via <see cref="DeletedAtUtc"/>.</summary>
public class PipelineProject : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    /// <summary>Planned completion date (calendar day only; no time component).</summary>
    public DateOnly? ExpectedDate { get; set; }

    public DateTime? DeletedAtUtc { get; set; }
}
