using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Prophet.Application.UserCases.Pipeline.ProjectFinalArtifacts;
using Prophet.Application.UserCases.Pipeline.ProjectInputs;
using Prophet.CrossCutting.ResultObjects;
using Prophet.CrossCutting.Validation;

namespace Prophet.Api.Controllers.Pipeline;

/// <summary>Final Markdown artifacts for a Prophet pipeline project (Firebase path genesis/prophet/{{projectId}}/final-artifacts). Completed pipelines also sync architecture, diagrams, and documentation here under fixed pipeline-*.md names.</summary>
[ApiController]
[Route("v1/prophet/projects/{projectId:guid}/final-artifacts")]
[Produces("application/json")]
[EnableRateLimiting("api")]
public class ProjectFinalArtifactsController(
    IListPipelineFinalArtifactsUseCase listUseCase,
    IUploadPipelineFinalArtifactsUseCase uploadUseCase,
    IDeletePipelineFinalArtifactUseCase deleteUseCase,
    IGetPipelineFinalArtifactDownloadUrlUseCase downloadUseCase,
    IGetPipelineFinalArtifactContentUseCase contentUseCase,
    IValidationErrorCollector errorCollector) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(Result<IReadOnlyList<PipelineFinalArtifactItemDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<IReadOnlyList<PipelineFinalArtifactItemDto>>>> List(
        Guid projectId,
        CancellationToken cancellationToken)
    {
        errorCollector.Clear();
        var items = await listUseCase.ExecuteAsync(projectId, cancellationToken);
        if (errorCollector.HasErrors)
            return BadRequest(Result<object>.Fail(errorCollector.GetErrors()));
        if (items == null)
            return NotFound();
        return Ok(Result<IReadOnlyList<PipelineFinalArtifactItemDto>>.Ok(items));
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(52_428_800)]
    [ProducesResponseType(typeof(Result<UploadPipelineFinalArtifactsResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<UploadPipelineFinalArtifactsResponseDto>>> Upload(
        Guid projectId,
        [FromForm(Name = "files")] List<IFormFile>? files,
        CancellationToken cancellationToken)
    {
        var fileList = files ?? new List<IFormFile>();
        if (fileList.Count == 0)
            return BadRequest(Result<object>.Fail(["No files were uploaded."]));
        if (fileList.Count > FinalArtifactUploadLimits.MaxFilesPerRequest)
            return BadRequest(Result<object>.Fail([$"At most {FinalArtifactUploadLimits.MaxFilesPerRequest} files per request."]));

        var chunks = new List<InputFileChunk>(fileList.Count);
        foreach (var f in fileList)
        {
            var name = f.FileName ?? "file";
            if (f.Length > FinalArtifactUploadLimits.MaxFileBytes)
            {
                chunks.Add(new InputFileChunk(
                    name,
                    f.ContentType ?? "application/octet-stream",
                    Array.Empty<byte>(),
                    $"File exceeds {FinalArtifactUploadLimits.MaxFileBytes / (1024 * 1024)} MB limit."));
                continue;
            }

            await using var stream = f.OpenReadStream();
            using var ms = new MemoryStream((int)f.Length);
            await stream.CopyToAsync(ms, cancellationToken).ConfigureAwait(false);
            chunks.Add(new InputFileChunk(
                name,
                f.ContentType ?? "application/octet-stream",
                ms.ToArray()));
        }

        errorCollector.Clear();
        var result = await uploadUseCase.ExecuteAsync(projectId, chunks, cancellationToken).ConfigureAwait(false);
        if (errorCollector.HasErrors)
            return BadRequest(Result<object>.Fail(errorCollector.GetErrors()));
        if (result == null)
            return NotFound();
        return Ok(Result<UploadPipelineFinalArtifactsResponseDto>.Ok(result));
    }

    /// <summary>Markdown body for preview. <c>/context</c> is an alias for the same handler (common typo).</summary>
    [HttpGet("{documentId:guid}/content")]
    [HttpGet("{documentId:guid}/context")]
    [ProducesResponseType(typeof(Result<PipelineFinalArtifactContentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<PipelineFinalArtifactContentDto>>> GetContent(
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
        return Ok(Result<PipelineFinalArtifactContentDto>.Ok(dto));
    }

    [HttpGet("{documentId:guid}/download")]
    [ProducesResponseType(typeof(Result<PipelineFinalArtifactDownloadDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<PipelineFinalArtifactDownloadDto>>> Download(
        Guid projectId,
        Guid documentId,
        CancellationToken cancellationToken)
    {
        errorCollector.Clear();
        var dto = await downloadUseCase.ExecuteAsync(projectId, documentId, cancellationToken).ConfigureAwait(false);
        if (errorCollector.HasErrors)
            return BadRequest(Result<object>.Fail(errorCollector.GetErrors()));
        if (dto == null)
            return NotFound();
        return Ok(Result<PipelineFinalArtifactDownloadDto>.Ok(dto));
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
