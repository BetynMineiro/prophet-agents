using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Prophet.Application.Interfaces.Diagnostics;
using Prophet.Application.Options;
using Prophet.Application.Services.Diagnostics;
using Prophet.Application.UserCases.Diagnostics;

namespace Prophet.Application;

/// <summary>Registers local diagnostics (this-host metrics + GET summary + SignalR) for Prophet/Seven API hosts. Abraham uses <see cref="ApplicationModule.AddApplication"/> which already includes the same primitives plus <see cref="LocalDiagnosticsOptions"/> binding.</summary>
public static class DiagnosticsApplicationModule
{
    public static void AddLocalDiagnosticsApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<DiagnosticsMetricsOptions>(configuration.GetSection(DiagnosticsMetricsOptions.SectionName));
        services.Configure<LocalDiagnosticsOptions>(configuration.GetSection(LocalDiagnosticsOptions.SectionName));
        services.AddSingleton<ProductMetricsAggregator>();
        services.AddSingleton<IProductMetricsRecorder>(sp => sp.GetRequiredService<ProductMetricsAggregator>());
        services.AddSingleton<IProductMetricsSnapshotProvider>(sp => sp.GetRequiredService<ProductMetricsAggregator>());
        services.AddScoped<IGetDiagnosticsSummaryUseCase, GetDiagnosticsSummaryUseCase>();
    }
}
