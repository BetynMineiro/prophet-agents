using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Prophet.CrossCutting.Middleware;

/// <summary>
/// Request/response logging (4.4): logs method, path, status code and duration per request.
/// Does not log request or response body (sanitized — no passwords/tokens).
/// </summary>
public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

    public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var method = context.Request.Method;
        var path = context.Request.Path.Value ?? string.Empty;

        await _next(context);

        stopwatch.Stop();
        var statusCode = context.Response.StatusCode;
        var elapsedMs = stopwatch.ElapsedMilliseconds;

        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation(
                "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {ElapsedMs} ms",
                method,
                path,
                statusCode,
                elapsedMs);
        }
    }
}
