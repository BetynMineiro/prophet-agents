using Microsoft.AspNetCore.SignalR;
using Prophet.Application.UserCases.Diagnostics;
using Prophet.Api.Hubs;

namespace Prophet.Api.Services;

public sealed class SignalRDiagnosticsBroadcaster(IHubContext<DiagnosticsHub> hub) : IDiagnosticsBroadcaster
{
    public Task PublishAsync(DiagnosticsSummaryDto summary, CancellationToken cancellationToken = default) =>
        hub.Clients.All.SendAsync(DiagnosticsHub.DiagnosticsUpdatedMethod, summary, cancellationToken);
}
