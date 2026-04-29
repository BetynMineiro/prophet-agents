namespace Prophet.Application.UserCases.Diagnostics;

/// <summary>API health entry for the diagnostics dashboard (liveness/ready).</summary>
public sealed record DiagnosticsApiStatusDto(
    string Id,
    string Name,
    string Liveness,
    string Ready
);

/// <summary>Per-product metrics row (requests/min, requests and errors in configured window).</summary>
public sealed record ProductDiagnosticsRowDto(
    Guid? ProductId,
    string ProductName,
    int RequestsPerMinute,
    int RequestsInWindow,
    int ErrorsInWindow,
    DateTime? LastActivityUtc,
    DateTime? LastErrorUtc
);

/// <summary>Full diagnostics summary: list of API statuses and per-product metrics.</summary>
public sealed record DiagnosticsSummaryDto(
    IReadOnlyList<DiagnosticsApiStatusDto> Apis,
    IReadOnlyList<ProductDiagnosticsRowDto> PerProduct
);
