namespace Prophet.Application.Options;

/// <summary>Configuration for diagnostics per-product metrics aggregator (rolling window).</summary>
public sealed class DiagnosticsMetricsOptions
{
    public const string SectionName = "Diagnostics:Metrics";

    /// <summary>Rolling window length in hours for request/error counts. Default 1.</summary>
    public double WindowHours { get; set; } = 1;
}
