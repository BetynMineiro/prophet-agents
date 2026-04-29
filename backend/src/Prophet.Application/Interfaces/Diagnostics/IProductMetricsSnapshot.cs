namespace Prophet.Application.Interfaces.Diagnostics;

/// <summary>Raw per-product metrics for one product (or Global when ProductId is null).</summary>
public sealed record ProductMetricsSnapshot(
    Guid? ProductId,
    int RequestsPerMinute,
    int RequestsInWindow,
    int ErrorsInWindow,
    DateTime? LastActivityUtc,
    DateTime? LastErrorUtc
);

/// <summary>Provides a snapshot of per-product request metrics for diagnostics.</summary>
public interface IProductMetricsSnapshotProvider
{
    IReadOnlyList<ProductMetricsSnapshot> GetSnapshot();
}
