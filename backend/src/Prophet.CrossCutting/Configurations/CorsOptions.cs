namespace Prophet.CrossCutting.Configurations;

/// <summary>
/// CORS policy options. Bind from config "Cors" section.
/// In non-Local, set Cors__AllowedOrigins__0, Cors__AllowedOrigins__1, etc. via pipeline/env.
/// </summary>
public class CorsOptions
{
    public const string SectionName = "Cors";

    /// <summary>Allowed origins (e.g. https://app.example.com). Defaults in appsettings for Local; override via env in Staging/Production.</summary>
    public string[] AllowedOrigins { get; set; } = [];

    /// <summary>
    /// First port of the localhost port range allowed in development (inclusive).
    /// When both <see cref="LocalhostPortFrom"/> and <see cref="LocalhostPortTo"/> are set,
    /// any http/https request from localhost or 127.0.0.1 with a port in [From, To] is accepted.
    /// Has no effect in production — use <see cref="AllowedOrigins"/> there.
    /// </summary>
    public int? LocalhostPortFrom { get; set; }

    /// <summary>Last port of the localhost port range allowed in development (inclusive).</summary>
    public int? LocalhostPortTo { get; set; }
}
