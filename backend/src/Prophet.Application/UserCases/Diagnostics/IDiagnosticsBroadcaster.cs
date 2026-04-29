namespace Prophet.Application.UserCases.Diagnostics;

/// <summary>Abstraction for pushing diagnostics summary to connected clients (e.g. SignalR).</summary>
public interface IDiagnosticsBroadcaster
{
    Task PublishAsync(DiagnosticsSummaryDto summary, CancellationToken cancellationToken = default);
}
