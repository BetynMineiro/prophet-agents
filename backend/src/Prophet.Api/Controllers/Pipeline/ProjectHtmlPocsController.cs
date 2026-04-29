using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Prophet.Application.UserCases.Pipeline.ProjectHtmlPocs;
using Prophet.Application.UserCases.Pipeline.ProjectInputs;
using Prophet.CrossCutting.ResultObjects;
using Prophet.CrossCutting.Validation;
using Prophet.Domain.Entities.Pipeline;

namespace Prophet.Api.Controllers.Pipeline;

/// <summary>HTML POCs (one Web + one Mobile slot per project; Firebase path genesis/prophet/{{projectId}}/html-pocs).</summary>
[ApiController]
[Route("v1/prophet/projects/{projectId:guid}/html-pocs")]
[Produces("application/json")]
[EnableRateLimiting("api")]
public class ProjectHtmlPocsController(
    IListPipelineHtmlPocsUseCase listUseCase,
    IUploadPipelineHtmlPocUseCase uploadUseCase,
    IDeletePipelineHtmlPocUseCase deleteUseCase,
    IGetPipelineHtmlPocSignedUrlUseCase signedUrlUseCase,
    IGetPipelineHtmlPocContentUseCase contentUseCase,
    IValidationErrorCollector errorCollector) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(Result<IReadOnlyList<PipelineHtmlPocItemDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<IReadOnlyList<PipelineHtmlPocItemDto>>>> List(
        Guid projectId,
        CancellationToken cancellationToken)
    {
        errorCollector.Clear();
        var items = await listUseCase.ExecuteAsync(projectId, cancellationToken);
        if (errorCollector.HasErrors)
            return BadRequest(Result<object>.Fail(errorCollector.GetErrors()));
        if (items == null)
            return NotFound();
        return Ok(Result<IReadOnlyList<PipelineHtmlPocItemDto>>.Ok(items));
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(52_428_800)]
    [ProducesResponseType(typeof(Result<PipelineHtmlPocItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Upload(
        Guid projectId,
        [FromForm] HtmlPocKind kind,
        [FromForm] bool replaceConfirmed,
        IFormFile? file,
        CancellationToken cancellationToken)
    {
        if (!Enum.IsDefined(kind))
            return BadRequest(Result<object>.Fail(["Invalid POC kind."]));
        if (file == null || file.Length == 0)
            return BadRequest(Result<object>.Fail(["No file was uploaded."]));
        var name = file.FileName ?? "file";
        if (!name.EndsWith(".html", StringComparison.OrdinalIgnoreCase) &&
            !name.EndsWith(".htm", StringComparison.OrdinalIgnoreCase))
            return BadRequest(Result<object>.Fail(["Only .html or .htm files are allowed."]));
        if (file.Length > HtmlPocUploadLimits.MaxFileBytes)
            return BadRequest(Result<object>.Fail([$"File exceeds {HtmlPocUploadLimits.MaxFileBytes / (1024 * 1024)} MB limit."]));

        await using var stream = file.OpenReadStream();
        using var ms = new MemoryStream((int)file.Length);
        await stream.CopyToAsync(ms, cancellationToken).ConfigureAwait(false);
        var chunk = new InputFileChunk(
            name,
            file.ContentType ?? "text/html",
            ms.ToArray());

        var result = await uploadUseCase.ExecuteAsync(projectId, kind, chunk, replaceConfirmed, cancellationToken).ConfigureAwait(false);
        if (result == null)
            return NotFound();
        if (result.RequiresReplaceConfirmation)
            return Conflict(Result<object>.Fail([HtmlPocConflictCodes.PocKindAlreadyExists]));
        if (result.Document == null)
            return BadRequest(Result<object>.Fail(["Upload failed."]));
        return Ok(Result<PipelineHtmlPocItemDto>.Ok(result.Document));
    }

    /// <summary>HTML body for popup preview. <c>/context</c> is an alias for the same handler (common typo).</summary>
    [HttpGet("{documentId:guid}/content")]
    [HttpGet("{documentId:guid}/context")]
    [ProducesResponseType(typeof(Result<PipelineHtmlPocContentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<PipelineHtmlPocContentDto>>> GetContent(
        Guid projectId,
        Guid documentId,
        CancellationToken cancellationToken)
    {
        errorCollector.Clear();
        var dto = await contentUseCase.ExecuteAsync(projectId, documentId, cancellationToken).ConfigureAwait(false);
        if (errorCollector.HasErrors)
            return BadRequest(Result<object>.Fail(errorCollector.GetErrors()));
        if (dto == null)
            return NotFound();
        return Ok(Result<PipelineHtmlPocContentDto>.Ok(dto));
    }

    [HttpGet("{documentId:guid}/download")]
    [ProducesResponseType(typeof(Result<PipelineHtmlPocDownloadDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<PipelineHtmlPocDownloadDto>>> Download(
        Guid projectId,
        Guid documentId,
        CancellationToken cancellationToken)
    {
        errorCollector.Clear();
        var dto = await signedUrlUseCase.ExecuteAsync(projectId, documentId, asAttachment: true, cancellationToken).ConfigureAwait(false);
        if (errorCollector.HasErrors)
            return BadRequest(Result<object>.Fail(errorCollector.GetErrors()));
        if (dto == null)
            return NotFound();
        return Ok(Result<PipelineHtmlPocDownloadDto>.Ok(dto));
    }

    [HttpGet("{documentId:guid}/view")]
    [ProducesResponseType(typeof(Result<PipelineHtmlPocDownloadDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<PipelineHtmlPocDownloadDto>>> View(
        Guid projectId,
        Guid documentId,
        CancellationToken cancellationToken)
    {
        errorCollector.Clear();
        var dto = await signedUrlUseCase.ExecuteAsync(projectId, documentId, asAttachment: false, cancellationToken).ConfigureAwait(false);
        if (errorCollector.HasErrors)
            return BadRequest(Result<object>.Fail(errorCollector.GetErrors()));
        if (dto == null)
            return NotFound();
        return Ok(Result<PipelineHtmlPocDownloadDto>.Ok(dto));
    }

    [HttpDelete("{documentId:guid}")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<bool>>> Delete(
        Guid projectId,
        Guid documentId,
        CancellationToken cancellationToken)
    {
        errorCollector.Clear();
        var ok = await deleteUseCase.ExecuteAsync(projectId, documentId, cancellationToken).ConfigureAwait(false);
        if (errorCollector.HasErrors)
            return BadRequest(Result<object>.Fail(errorCollector.GetErrors()));
        if (!ok)
            return NotFound();
        return Ok(Result<bool>.Ok(true));
    }
}
