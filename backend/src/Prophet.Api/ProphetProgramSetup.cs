using System.Threading.RateLimiting;
using Prophet.CrossCutting.Configurations;

namespace Prophet.Api;

internal static class ProphetProgramSetup
{
    internal static void AddRateLimiterPolicies(WebApplicationBuilder builder)
    {
        var rateLimitApi = builder.Configuration.GetSection("RateLimit:Api").Get<RateLimitOptions>() ?? new RateLimitOptions { PermitLimit = 100, WindowSeconds = 60 };
        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy("api", context =>
            {
                var key = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                return RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = rateLimitApi.PermitLimit,
                    Window = TimeSpan.FromSeconds(rateLimitApi.WindowSeconds),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 0
                });
            });
        });
    }

    internal static void AddCorsPolicy(WebApplicationBuilder builder)
    {
        var corsOptions = builder.Configuration.GetSection(CorsOptions.SectionName).Get<CorsOptions>() ?? new CorsOptions();
        var allowedOrigins = corsOptions.AllowedOrigins
            .Where(o => !string.IsNullOrWhiteSpace(o))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var portFrom = corsOptions.LocalhostPortFrom;
        var portTo = corsOptions.LocalhostPortTo;
        var hasRange = portFrom.HasValue && portTo.HasValue;

        bool IsOriginAllowed(string origin)
        {
            if (allowedOrigins.Contains(origin)) return true;
            if (!hasRange) return false;
            if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri)) return false;
            var isLocalhost = uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
                           || uri.Host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase);
            return isLocalhost && uri.Port >= portFrom!.Value && uri.Port <= portTo!.Value;
        }

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("DefaultCors", corsPolicyBuilder =>
            {
                if (allowedOrigins.Count > 0 || hasRange)
                    corsPolicyBuilder.SetIsOriginAllowed(IsOriginAllowed).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
                else
                    corsPolicyBuilder.SetIsOriginAllowed(_ => false).AllowAnyHeader().AllowAnyMethod();
            });
        });
    }

    internal static void EnsureRequiredConfiguration(WebApplication app)
    {
        var isTesting = string.Equals(app.Configuration["Testing:UseInMemoryDatabase"], "true", StringComparison.OrdinalIgnoreCase);
        if (isTesting)
            return;

        var conn = ProphetConnectionStrings.GetPostgreSql(app.Configuration);
        var isDev = app.Environment.IsDevelopment();

        if (string.IsNullOrWhiteSpace(conn) && !isDev)
            throw new InvalidOperationException(
                "Set ConnectionStrings__postgresdb or ConnectionStrings__Default via environment or vault.");

        if (isDev && string.IsNullOrWhiteSpace(conn))
        {
            app.Logger.LogWarning("Prophet: No connection string configured. Using in-memory database for development.");
        }
    }
}
