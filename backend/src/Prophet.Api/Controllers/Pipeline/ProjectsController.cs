using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Prophet.Application.UserCases.Pipeline.Projects;
using Prophet.CrossCutting.RequestObjects;
using Prophet.CrossCutting.ResultObjects;
using Prophet.CrossCutting.Validation;

namespace Prophet.Api.Controllers.Pipeline;

/// <summary>Pipeline projects. Bearer JWT issued by Abraham; same issuer, audience, and signing key as Prophet configuration.</summary>
[ApiController]
[Route("v1/prophet/projects")]
[Produces("application/json")]
[EnableRateLimiting("api")]
public class ProjectsController(
    ICreatePipelineProjectUseCase createUseCase,
    IUpdatePipelineProjectUseCase updateUseCase,
    IListPipelineProjectsUseCase listUseCase,
    IGetPipelineProjectUseCase getUseCase,
    IDeletePipelineProjectUseCase deleteUseCase,
    IRestorePipelineProjectUseCase restoreUseCase,
    IValidationErrorCollector errorCollector) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(Result<PipelineProjectItemDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Result<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Result<PipelineProjectItemDto>>> Create([FromBody] CreatePipelineProjectRequest request, CancellationToken cancellationToken)
    {
        var item = await createUseCase.ExecuteAsync(request, cancellationToken);
        if (item == null)
            return Ok(Result<PipelineProjectItemDto>.Ok(null!));
        return CreatedAtAction(nameof(Get), new { id = item.Id }, Result<PipelineProjectItemDto>.Ok(item));
    }

    /// <summary>Update project name, description and expected date.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(Result<PipelineProjectItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<PipelineProjectItemDto>>> Update(Guid id, [FromBody] UpdatePipelineProjectRequest request, CancellationToken cancellationToken)
    {
        errorCollector.Clear();
        var item = await updateUseCase.ExecuteAsync(id, request, cancellationToken);
        if (errorCollector.HasErrors)
            return BadRequest(Result<object>.Fail(errorCollector.GetErrors()));
        if (item == null)
            return NotFound();
        return Ok(Result<PipelineProjectItemDto>.Ok(item));
    }

    /// <summary>List projects (cursor pagination). Query: pageSize (1–500), cursor, searchText, activeState (All|Active|Inactive).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(Result<CursorPage<PipelineProjectItemDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<Result<CursorPage<PipelineProjectItemDto>>>> List(
        [FromQuery] int? pageSize,
        [FromQuery] string? cursor,
        [FromQuery] string? searchText,
        [FromQuery] ActiveState activeState = ActiveState.All,
        CancellationToken cancellationToken = default)
    {
        var size = pageSize is >= 1 and <= 500 ? pageSize.Value : 10;
        if (!string.IsNullOrWhiteSpace(cursor) && !Guid.TryParse(cursor, out _))
            return Ok(Result<CursorPage<PipelineProjectItemDto>>.Fail(["Cursor must be a valid GUID."]));
        var request = new PagedRequest
        {
            PageSize = size,
            Cursor = string.IsNullOrWhiteSpace(cursor) ? null : cursor.Trim(),
            SearchText = string.IsNullOrWhiteSpace(searchText) ? null : searchText.Trim(),
            ActiveState = activeState
        };
        var page = await listUseCase.ExecuteAsync(request, cancellationToken);
        return Ok(Result<CursorPage<PipelineProjectItemDto>>.Ok(page));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(Result<PipelineProjectItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<PipelineProjectItemDto>>> Get(Guid id, CancellationToken cancellationToken)
    {
        errorCollector.Clear();
        var item = await getUseCase.ExecuteAsync(id, cancellationToken);
        if (errorCollector.HasErrors)
            return BadRequest(Result<object>.Fail(errorCollector.GetErrors()));
        if (item == null)
            return NotFound();
        return Ok(Result<PipelineProjectItemDto>.Ok(item));
    }

    /// <summary>Restore a soft-deleted project (clears deletion timestamp).</summary>
    [HttpPatch("{id:guid}/restore")]
    [ProducesResponseType(typeof(Result<PipelineProjectItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<PipelineProjectItemDto>>> Restore(Guid id, CancellationToken cancellationToken)
    {
        errorCollector.Clear();
        var item = await restoreUseCase.ExecuteAsync(id, cancellationToken);
        if (errorCollector.HasErrors)
            return BadRequest(Result<object>.Fail(errorCollector.GetErrors()));
        if (item == null)
            return NotFound();
        return Ok(Result<PipelineProjectItemDto>.Ok(item));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<bool>>> Delete(Guid id, CancellationToken cancellationToken)
    {
        errorCollector.Clear();
        var ok = await deleteUseCase.ExecuteAsync(id, cancellationToken);
        if (errorCollector.HasErrors)
            return BadRequest(Result<object>.Fail(errorCollector.GetErrors()));
        if (!ok)
            return NotFound();
        return Ok(Result<bool>.Ok(true));
    }
}
