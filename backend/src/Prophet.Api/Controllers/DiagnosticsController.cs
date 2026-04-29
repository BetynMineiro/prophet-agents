using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Prophet.Application.UserCases.Diagnostics;
using Prophet.CrossCutting.ResultObjects;

namespace Prophet.Api.Controllers;

[ApiController]
[Route("v1/prophet/diagnostics")]
[Produces("application/json")]
[EnableRateLimiting("api")]
public sealed class DiagnosticsController(IGetDiagnosticsSummaryUseCase getSummaryUseCase) : ControllerBase
{
    /// <summary>Diagnostics for this Prophet API host only (health + per-product metrics on this process).</summary>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(Result<DiagnosticsSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<Result<DiagnosticsSummaryDto>>> GetSummary(CancellationToken cancellationToken)
    {
        var dto = await getSummaryUseCase.ExecuteAsync(cancellationToken);
        return Ok(Result<DiagnosticsSummaryDto>.Ok(dto));
    }
}
