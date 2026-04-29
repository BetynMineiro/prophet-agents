using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using Prophet.Application.Interfaces.Diagnostics;
using Prophet.Application.Options;

namespace Prophet.Application.Services.Diagnostics;

/// <summary>In-memory aggregator: records requests per product in minute buckets (configurable window). Thread-safe.</summary>
public sealed class ProductMetricsAggregator : IProductMetricsRecorder, IProductMetricsSnapshotProvider
{
    private const long TicksPerMinute = 600_000_000L;

    /// <summary>Key for "Global" (requests without product, e.g. health/hubs).</summary>
    private static readonly Guid GlobalKey = Guid.Empty;

    private readonly ConcurrentDictionary<Guid, ProductMinuteBuckets> _perProduct = new();
    private readonly DiagnosticsMetricsOptions _options;

    public ProductMetricsAggregator(IOptions<DiagnosticsMetricsOptions>? options)
    {
        _options = options?.Value ?? new DiagnosticsMetricsOptions();
    }

    public void RecordRequest(Guid? productId, bool isError)
    {
        var key = productId ?? GlobalKey;
        var buckets = _perProduct.GetOrAdd(key, _ => new ProductMinuteBuckets(() => GetWindowMinutes));
        var now = DateTime.UtcNow;
        var minuteTicks = now.Ticks / TicksPerMinute * TicksPerMinute;

        buckets.Record(minuteTicks, isError, now);
    }

    public IReadOnlyList<ProductMetricsSnapshot> GetSnapshot()
    {
        var now = DateTime.UtcNow;
        var window = TimeSpan.FromHours(Math.Max(0.1, _options.WindowHours));
        var cutoff = now - window;
        var cutoffTicks = cutoff.Ticks / TicksPerMinute * TicksPerMinute;
        var lastMinuteTicks = now.Ticks / TicksPerMinute * TicksPerMinute;
        var result = new List<ProductMetricsSnapshot>();

        foreach (var kv in _perProduct.ToArray())
        {
            var key = kv.Key;
            var buckets = kv.Value;
            var (perMin, totalInWindow, errorsInWindow, lastActivity, lastError) =
                buckets.GetStats(cutoffTicks, lastMinuteTicks);
            var productId = key == GlobalKey ? (Guid?)null : key;
            result.Add(new ProductMetricsSnapshot(
                productId,
                perMin,
                (int)Math.Min(totalInWindow, int.MaxValue),
                (int)Math.Min(errorsInWindow, int.MaxValue),
                lastActivity,
                lastError
            ));
        }

        return result.OrderBy(x => x.ProductId == null ? 0 : 1).ThenBy(x => x.ProductId ?? Guid.Empty).ToList();
    }

    private long GetWindowMinutes => (long)Math.Max(1, Math.Ceiling(_options.WindowHours * 60));

    private sealed class ProductMinuteBuckets
    {
        private readonly ConcurrentDictionary<long, MinuteBucket> _buckets = new();
        private readonly object _pruneLock = new();
        private readonly Func<long> _getWindowMinutes;

        public ProductMinuteBuckets(Func<long> getWindowMinutes)
        {
            _getWindowMinutes = getWindowMinutes;
        }

        public void Record(long minuteTicks, bool isError, DateTime now)
        {
            _ = _buckets.AddOrUpdate(minuteTicks,
            _ => new MinuteBucket(1, isError ? 1L : 0, now, isError ? now : null),
            (_, existing) => existing.Add(1, isError, now));

            PruneIfNeeded(minuteTicks);
        }

        private void PruneIfNeeded(long currentMinuteTicks)
        {
            if (_buckets.Count < 1500) return;
            lock (_pruneLock)
            {
                var windowMinutes = _getWindowMinutes();
                var cutoff = currentMinuteTicks - windowMinutes * TicksPerMinute;
                foreach (var key in _buckets.Keys.Where(k => k < cutoff).ToArray())
                    _buckets.TryRemove(key, out _);
            }
        }

        public (int RequestsPerMinute, long TotalInWindow, long ErrorsInWindow, DateTime? LastActivity, DateTime? LastError) GetStats(
            long cutoffTicks, long lastMinuteTicks)
        {
            long totalInWindow = 0, errorsInWindow = 0;
            int perMin = 0;
            DateTime? lastActivity = null, lastError = null;

            foreach (var kv in _buckets.ToArray())
            {
                var ticks = kv.Key;
                var b = kv.Value;
                if (ticks < cutoffTicks) continue;
                totalInWindow += b.Total;
                errorsInWindow += b.Errors;
                if (b.LastActivityUtc != null && (lastActivity == null || b.LastActivityUtc > lastActivity))
                    lastActivity = b.LastActivityUtc;
                if (b.LastErrorUtc != null && (lastError == null || b.LastErrorUtc > lastError))
                    lastError = b.LastErrorUtc;
                if (ticks == lastMinuteTicks)
                    perMin = (int)Math.Min(b.Total, int.MaxValue);
            }

            return (perMin, totalInWindow, errorsInWindow, lastActivity, lastError);
        }
    }

    private sealed class MinuteBucket
    {
        public long Total { get; }
        public long Errors { get; }
        public DateTime? LastActivityUtc { get; }
        public DateTime? LastErrorUtc { get; }

        public MinuteBucket(long total, long errors, DateTime lastActivity, DateTime? lastError)
        {
            Total = total;
            Errors = errors;
            LastActivityUtc = lastActivity;
            LastErrorUtc = lastError;
        }

        public MinuteBucket Add(long count, bool isError, DateTime now)
        {
            return new MinuteBucket(
                Total + count,
                Errors + (isError ? 1 : 0),
                now,
                isError ? now : LastErrorUtc
            );
        }
    }
}
