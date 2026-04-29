using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Prophet.Application.Interfaces.Diagnostics;
using Prophet.CrossCutting.Diagnostics;

namespace Prophet.Application.Middleware;

/// <summary>Records request metrics per product (product id in Items) after the response for diagnostics dashboard.</summary>
public sealed class RequestMetricsMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        await next(context);

        var recorder = context.RequestServices.GetService<IProductMetricsRecorder>();
        if (recorder == null)
            return;

        var productId = context.Items[HttpContextDiagnosticsKeys.ProductId] as Guid?;
        var statusCode = context.Response.StatusCode;
        var isError = statusCode >= 400;

        recorder.RecordRequest(productId, isError);
    }
}
