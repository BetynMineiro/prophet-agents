using System.Net;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Prophet.Application.Interfaces.Diagnostics;
using Prophet.Application.Options;

namespace Prophet.Application.UserCases.Diagnostics;

/// <summary>
/// Builds diagnostics for <b>this</b> API host only (one row in <see cref="DiagnosticsSummaryDto.Apis"/>).
/// Other APIs expose their own GET .../diagnostics/summary and SignalR /hubs/diagnostics; the frontend aggregates.
/// </summary>
public sealed class GetDiagnosticsSummaryUseCase(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    IOptions<LocalDiagnosticsOptions> localOptions,
    IProductMetricsSnapshotProvider metricsSnapshot,
    ILogger<GetDiagnosticsSummaryUseCase> logger) : IGetDiagnosticsSummaryUseCase
{
    private const string HealthStatusHealthy = "Healthy";

    public async Task<DiagnosticsSummaryDto> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient("diagnostics-http");
        client.Timeout = TimeSpan.FromSeconds(5);

        var (id, name, baseUrl) = ResolveLocalApi();
        baseUrl = baseUrl.TrimEnd('/');

        var live = await ProbeHealthAsync(client, $"{baseUrl}/health", cancellationToken);
        var ready = await ProbeHealthAsync(client, $"{baseUrl}/health/ready", cancellationToken);

        if (!live.Ok || !ready.Ok)
        {
            logger.LogWarning(
                "Diagnostics health check failed for {ApiName} ({ApiId}) at {BaseUrl}. Liveness: {Liveness}; Readiness: {Readiness}",
                name,
                id,
                baseUrl,
                live.Ok ? HealthStatusHealthy : $"Unhealthy ({live.Detail})",
                ready.Ok ? HealthStatusHealthy : $"Unhealthy ({ready.Detail})");
        }

        var apis = new List<DiagnosticsApiStatusDto>
        {
            new(
                id,
                name,
                live.Ok ? HealthStatusHealthy : "Unhealthy",
                ready.Ok ? HealthStatusHealthy : "Unhealthy"),
        };

        var perProduct = BuildPerProductRows(metricsSnapshot.GetSnapshot());

        return new DiagnosticsSummaryDto(apis, perProduct);
    }

    private (string Id, string Name, string BaseUrl) ResolveLocalApi()
    {
        var o = localOptions.Value;
        var baseUrl = (o.BaseUrl ?? "").Trim();
        if (string.IsNullOrWhiteSpace(baseUrl))
            baseUrl = ResolveBaseUrlFromConfigurationAndHost();

        var id = (o.ApiId ?? "").Trim();
        var name = (o.ApiName ?? "").Trim();
        if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(name))
        {
            var inferred = InferIdAndNameFromEntryAssembly();
            if (string.IsNullOrWhiteSpace(id))
                id = inferred.Id;
            if (string.IsNullOrWhiteSpace(name))
                name = inferred.Name;
        }

        return (id, name, baseUrl);
    }

    private string ResolveBaseUrlFromConfigurationAndHost()
    {
        var entry = Assembly.GetEntryAssembly()?.GetName().Name ?? "";
        if (entry.Contains("Prophet", StringComparison.Ordinal))
        {
            return configuration["Diagnostics:ProphetBaseUrl"]
                ?? configuration["Urls:ProphetApi"]
                ?? "https://localhost:7252";
        }

        if (entry.Contains("Seven", StringComparison.Ordinal))
        {
            return configuration["Diagnostics:SevenBaseUrl"]
                ?? configuration["Urls:SevenApi"]
                ?? "https://localhost:7274";
        }

        return configuration["Diagnostics:GenesisBaseUrl"]
            ?? configuration["Urls:GenesisApi"]
            ?? configuration["Diagnostics:AbrahamBaseUrl"]
            ?? "https://localhost:7015";
    }

    private static (string Id, string Name) InferIdAndNameFromEntryAssembly()
    {
        var entry = Assembly.GetEntryAssembly()?.GetName().Name ?? "";
        if (entry.Contains("Prophet", StringComparison.Ordinal))
            return ("prophet", "Prophet API");
        if (entry.Contains("Seven", StringComparison.Ordinal))
            return ("seven", "Seven API");
        return ("genesis", "Genesis API");
    }

    private static List<ProductDiagnosticsRowDto> BuildPerProductRows(
        IReadOnlyList<ProductMetricsSnapshot> snapshot)
    {
        var rows = new List<ProductDiagnosticsRowDto>();
        foreach (var s in snapshot)
        {
            var productName = s.ProductId?.ToString() ?? "Global";
            rows.Add(new ProductDiagnosticsRowDto(
                s.ProductId,
                productName,
                s.RequestsPerMinute,
                s.RequestsInWindow,
                s.ErrorsInWindow,
                s.LastActivityUtc,
                s.LastErrorUtc
            ));
        }
        return rows;
    }

    private static async Task<(bool Ok, string Detail)> ProbeHealthAsync(
        HttpClient client,
        string url,
        CancellationToken ct)
    {
        try
        {
            using var resp = await client.GetAsync(url, ct);
            if (resp.StatusCode == HttpStatusCode.OK)
                return (true, "");
            return (false, $"HTTP {(int)resp.StatusCode} {resp.ReasonPhrase ?? ""}".TrimEnd());
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }
}
