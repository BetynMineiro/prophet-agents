using System.Collections.Concurrent;
using System.Diagnostics.Metrics;

namespace Prophet.CrossCutting.Metrics;

/// <summary>
/// OpenTelemetry-backed implementation of <see cref="IMetrics"/>. Creates counters by name on first use; thread-safe.
/// </summary>
public sealed class MetricsService : IMetrics
{
    private static readonly Meter Meter = new(MetricsMeterNames.Business);
    private readonly ConcurrentDictionary<string, Counter<long>> _counters = new();

    public void RecordCounter(string counterName, IReadOnlyDictionary<string, object?> tags)
    {
        var counter = _counters.GetOrAdd(counterName, name =>
            Meter.CreateCounter<long>(name, description: $"Business counter: {name}."));
        var tagList = tags.Select(t => new KeyValuePair<string, object?>(t.Key, t.Value)).ToArray();
        counter.Add(1, tagList);
    }
}
