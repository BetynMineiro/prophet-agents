using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Prophet.Application.UserCases.Pipeline.ProjectInputs;
using Prophet.Application.UserCases.Pipeline.ProjectInputs.Validators;
using Prophet.CrossCutting.ResultObjects;
using Prophet.CrossCutting.Validation;
using Prophet.Domain.Entities.Pipeline;

namespace Prophet.Api.Controllers.Pipeline;

/// <summary>Input documents for a Prophet pipeline project (Firebase path genesis/prophet/{{projectId}}/inputs).</summary>
[ApiController]
[Route("v1/prophet/projects/{projectId:guid}/inputs")]
[Produces("application/json")]
[EnableRateLimiting("api")]
public class ProjectInputsController(
    IListPipelineInputDocumentsUseCase listUseCase,
    IUploadPipelineInputDocumentsUseCase uploadUseCase,
    IDeletePipelineInputDocumentUseCase deleteUseCase,
    IGetPipelineInputDownloadUrlUseCase downloadUseCase,
    IValidationErrorCollector errorCollector) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(Result<IReadOnlyList<PipelineInputDocumentItemDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<IReadOnlyList<PipelineInputDocumentItemDto>>>> List(
        Guid projectId,
        CancellationToken cancellationToken)
    {
        errorCollector.Clear();
        var items = await listUseCase.ExecuteAsync(projectId, cancellationToken);
        if (errorCollector.HasErrors)
            return BadRequest(Result<object>.Fail(errorCollector.GetErrors()));
        if (items == null)
            return NotFound();
        return Ok(Result<IReadOnlyList<PipelineInputDocumentItemDto>>.Ok(items));
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(52_428_800)]
    [ProducesResponseType(typeof(Result<UploadPipelineInputDocumentsResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<UploadPipelineInputDocumentsResponseDto>>> Upload(
        Guid projectId,
        [FromForm(Name = "files")] List<IFormFile>? files,
        CancellationToken cancellationToken)
    {
        var fileList = files ?? new List<IFormFile>();
        if (fileList.Count == 0)
            return BadRequest(Result<object>.Fail(["No files were uploaded."]));
        if (fileList.Count > UploadPipelineInputDocumentsRequestValidator.MaxFilesPerRequest)
            return BadRequest(Result<object>.Fail([$"At most {UploadPipelineInputDocumentsRequestValidator.MaxFilesPerRequest} files per request."]));

        var chunks = new List<InputFileChunk>(fileList.Count);
        foreach (var f in fileList)
        {
            var name = f.FileName ?? "file";
            if (!PipelineTextInputFileRules.IsAllowedExtension(name))
            {
                chunks.Add(new InputFileChunk(
                    name,
                    f.ContentType ?? "application/octet-stream",
                    Array.Empty<byte>(),
                    $"Extension not allowed. Allowed types include {PipelineTextInputFileRules.AllowedHint}."));
                continue;
            }

            if (f.Length > UploadPipelineInputDocumentsUseCase.MaxFileBytes)
            {
                chunks.Add(new InputFileChunk(
                    name,
                    f.ContentType ?? "application/octet-stream",
                    Array.Empty<byte>(),
                    $"File exceeds {UploadPipelineInputDocumentsUseCase.MaxFileBytes / (1024 * 1024)} MB limit."));
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
        return Ok(Result<UploadPipelineInputDocumentsResponseDto>.Ok(result));
    }

    [HttpGet("{documentId:guid}/download")]
    [ProducesResponseType(typeof(Result<PipelineInputDownloadDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<PipelineInputDownloadDto>>> Download(
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
        return Ok(Result<PipelineInputDownloadDto>.Ok(dto));
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
