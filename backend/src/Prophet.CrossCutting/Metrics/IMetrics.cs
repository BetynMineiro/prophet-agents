namespace Prophet.CrossCutting.Metrics;

/// <summary>
/// Generic metrics recording for OpenTelemetry. Implementations record counters with tags (e.g. for auth, orders).
/// Reusable across domains; domain-specific ports (e.g. IAuthMetrics) in Application delegate to this.
/// </summary>
public interface IMetrics
{
    /// <summary>
    /// Records a counter increment with the given name and tags.
    /// </summary>
    void RecordCounter(string counterName, IReadOnlyDictionary<string, object?> tags);
}
