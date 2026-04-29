using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Prophet.CrossCutting.Middleware;

/// <summary>
/// Ensures each request has an Activity (when none exists) and adds TraceId/SpanId to the logging scope (5.10)
/// so all logs in that request include correlation ids for tracing.
/// </summary>
public class TraceIdLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TraceIdLoggingMiddleware> _logger;

    public TraceIdLoggingMiddleware(RequestDelegate next, ILogger<TraceIdLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        Activity? startedActivity = null;
        if (Activity.Current == null)
        {
            startedActivity = new Activity("Genesis.HttpRequest");
            startedActivity.Start();
        }

        try
        {
            var activity = Activity.Current!;
            using (_logger.BeginScope(new Dictionary<string, object?>
            {
                ["TraceId"] = activity.TraceId.ToString(),
                ["SpanId"] = activity.SpanId.ToString()
            }))
            {
                await _next(context);
            }
        }
        finally
        {
            startedActivity?.Stop();
        }
    }
}
