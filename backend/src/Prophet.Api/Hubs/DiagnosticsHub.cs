using Microsoft.AspNetCore.SignalR;

namespace Prophet.Api.Hubs;

/// <summary>SignalR hub for this host's diagnostics. Clients subscribe to <see cref="DiagnosticsUpdatedMethod"/>.</summary>
public sealed class DiagnosticsHub : Hub
{
    public const string DiagnosticsUpdatedMethod = "diagnosticsUpdated";
}
