using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Prophet.Application.UserCases.Pipeline.PipelineExecution;
using Prophet.CrossCutting.RequestObjects;
using Prophet.CrossCutting.ResultObjects;

namespace Prophet.Api.Controllers.Pipeline;

/// <summary>Artifact versions (§5 ARTIFACT_VERSIONS, §7).</summary>
[ApiController]
[Route("v1/prophet/projects/{projectId:guid}/versions")]
[Produces("application/json")]
[EnableRateLimiting("api")]
public class ProjectArtifactVersionsController(
    ICreateArtifactVersionUseCase createUseCase,
    IListArtifactVersionsUseCase listUseCase,
    IGetArtifactVersionUseCase getVersionUseCase) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(Result<CursorPage<ArtifactVersionItemDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<Result<CursorPage<ArtifactVersionItemDto>>>> List(
        Guid projectId,
        [FromQuery] int? pageSize,
        [FromQuery] string? cursor,
        CancellationToken cancellationToken)
    {
        var size = pageSize is >= 1 and <= 500 ? pageSize.Value : 20;
        var page = await listUseCase.ExecuteAsync(
            projectId,
            new PagedRequest { PageSize = size, Cursor = string.IsNullOrWhiteSpace(cursor) ? null : cursor.Trim() },
            cancellationToken);
        if (page == null)
            return NotFound();
        return Ok(Result<CursorPage<ArtifactVersionItemDto>>.Ok(page));
    }

    [HttpGet("{versionId:guid}")]
    [ProducesResponseType(typeof(Result<ArtifactVersionItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<ArtifactVersionItemDto>>> GetById(
        Guid projectId,
        Guid versionId,
        CancellationToken cancellationToken)
    {
        var item = await getVersionUseCase.ExecuteAsync(projectId, versionId, cancellationToken);
        if (item == null)
            return NotFound();
        return Ok(Result<ArtifactVersionItemDto>.Ok(item));
    }

    [HttpPost]
    [ProducesResponseType(typeof(Result<ArtifactVersionItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<ArtifactVersionItemDto>>> Create(
        Guid projectId,
        [FromBody] CreateArtifactVersionRequest request,
        CancellationToken cancellationToken)
    {
        var item = await createUseCase.ExecuteAsync(projectId, request, cancellationToken);
        if (item == null)
            return NotFound();
        return Ok(Result<ArtifactVersionItemDto>.Ok(item));
    }
}
