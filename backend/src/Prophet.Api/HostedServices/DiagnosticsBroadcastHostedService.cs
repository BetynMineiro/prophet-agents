using Prophet.Application.UserCases.Diagnostics;

namespace Prophet.Api.HostedServices;

public sealed class DiagnosticsBroadcastHostedService(
    IServiceProvider serviceProvider,
    ILogger<DiagnosticsBroadcastHostedService> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(15);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var getSummary = scope.ServiceProvider.GetRequiredService<IGetDiagnosticsSummaryUseCase>();
                var broadcaster = scope.ServiceProvider.GetRequiredService<IDiagnosticsBroadcaster>();

                var summary = await getSummary.ExecuteAsync(stoppingToken);
                await broadcaster.PublishAsync(summary, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Diagnostics broadcast failed");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }
}
