namespace Prophet.CrossCutting.Metrics;

/// <summary>
/// Meter names registered with OpenTelemetry (AddMeter). Used by CrossCuttingModule and MetricsService.
/// </summary>
public static class MetricsMeterNames
{
    /// <summary>
    /// Business metrics (auth, future domains). Single meter for all custom counters.
    /// </summary>
    public const string Business = "Genesis.Metrics";
}
