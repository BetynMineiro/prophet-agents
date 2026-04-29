namespace Prophet.Application.UserCases.Diagnostics;

public interface IGetDiagnosticsSummaryUseCase
{
    Task<DiagnosticsSummaryDto> ExecuteAsync(CancellationToken cancellationToken = default);
}
