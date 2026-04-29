namespace Prophet.CrossCutting.Diagnostics;

/// <summary>Keys shared by identity/metrics middleware for per-product request metrics.</summary>
public static class HttpContextDiagnosticsKeys
{
    /// <summary>Validated product id for the current request (diagnostics / RequestMetricsMiddleware).</summary>
    public const string ProductId = "ProductId";
}
