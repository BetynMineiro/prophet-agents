namespace Prophet.CrossCutting.Configurations;

/// <summary>
/// Rate limit policy options. Bind from config "RateLimit:Api" and "RateLimit:Auth".
/// </summary>
public class RateLimitOptions
{
    /// <summary>Max requests per window. Default 100 for api, 15 for auth.</summary>
    public int PermitLimit { get; set; } = 100;

    /// <summary>Window duration in seconds. Default 60.</summary>
    public int WindowSeconds { get; set; } = 60;
}
