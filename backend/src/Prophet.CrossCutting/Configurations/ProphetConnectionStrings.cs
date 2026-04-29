using Microsoft.Extensions.Configuration;

namespace Prophet.CrossCutting.Configurations;

/// <summary>
/// Shared PostgreSQL connection for Genesis APIs (Abraham, Prophet). Same database and EF migrations assembly.
/// Order matches Abraham <c>ApiModule</c>: environment/appsettings <c>postgresdb</c>, then <c>Default</c>.
/// Override with <c>ConnectionStrings__postgresdb</c> or <c>ConnectionStrings__Default</c>.
/// </summary>
public static class ProphetConnectionStrings
{
    /// <summary>First non-empty value wins (empty JSON entries are ignored so <c>Default</c> still applies).</summary>
    public static string? GetPostgreSql(IConfiguration configuration)
    {
        var pg = configuration.GetConnectionString("postgresdb");
        if (!string.IsNullOrWhiteSpace(pg))
            return pg.Trim();
        var def = configuration.GetConnectionString("Default");
        return string.IsNullOrWhiteSpace(def) ? null : def.Trim();
    }
}
