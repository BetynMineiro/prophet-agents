using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prophet.CrossCutting.ResultObjects;

namespace Prophet.CrossCutting.Middleware;

/// <summary>
/// Catches unhandled exceptions and returns a standardized <see cref="Result{T}"/> with appropriate status code.
/// In non-Development environments (4.5), only generic messages are returned; exception details are never exposed.
/// </summary>
public class ErrorHandlerMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlerMiddleware> _logger;
    private readonly IHostEnvironment _hostEnvironment;

    public ErrorHandlerMiddleware(RequestDelegate next, ILogger<ErrorHandlerMiddleware> logger, IHostEnvironment hostEnvironment)
    {
        _next = next;
        _logger = logger;
        _hostEnvironment = hostEnvironment;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception. ExceptionType={ExceptionType}, Message={Message}",
                ex.GetType().FullName,
                ex.Message);
            await HandleExceptionAsync(httpContext, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var isDevelopment = _hostEnvironment.IsDevelopment();
        var (statusCode, message) = exception switch
        {
            HttpRequestException => (503, "External API is temporarily unavailable."),
            ArgumentException => (400, isDevelopment ? exception.Message : "Invalid request."),
            _ => (500, "Internal Server Error.")
        };

        context.Response.StatusCode = statusCode;
        var result = Result<object>.Fail(message, statusCode);
        return context.Response.WriteAsync(JsonSerializer.Serialize(result, JsonOptions));
    }
}
