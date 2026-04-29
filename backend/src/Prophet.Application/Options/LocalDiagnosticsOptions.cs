namespace Prophet.Application.Options;

/// <summary>
/// Identifies this running API host for diagnostics (health probe + per-host metrics + SignalR).
/// Each API process must set its own <see cref="BaseUrl"/> (public URL used to call /health and /health/ready).
/// </summary>
public sealed class LocalDiagnosticsOptions
{
    public const string SectionName = "Diagnostics:Local";

    /// <summary>Stable id for dashboards (e.g. genesis, prophet, seven).</summary>
    public string ApiId { get; set; } = "genesis";

    /// <summary>Display name for dashboards.</summary>
    public string ApiName { get; set; } = "Genesis API";

    /// <summary>Public base URL of this API (no trailing slash), e.g. https://localhost:7015.</summary>
    public string BaseUrl { get; set; } = "";
}
