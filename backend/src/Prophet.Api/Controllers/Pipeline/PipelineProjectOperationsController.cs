using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Prophet.Application.UserCases.Pipeline.PipelineExecution;
using Prophet.CrossCutting.ResultObjects;
using Prophet.Domain.Entities.Pipeline;

namespace Prophet.Api.Controllers.Pipeline;

/// <summary>Project-level upload and refinement (§7).</summary>
[ApiController]
[Route("v1/prophet/projects/{projectId:guid}")]
[Produces("application/json")]
[EnableRateLimiting("api")]
public class PipelineProjectOperationsController(
    IUploadPipelineInputUseCase uploadUseCase,
    IRefinePipelineProjectUseCase refineUseCase) : ControllerBase
{
    /// <summary>Upload input document; optional <paramref name="versionId"/> — when omitted, a new artifact version is created.</summary>
    [HttpPost("upload")]
    [ProducesResponseType(typeof(Result<PipelineVersionFileItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Upload(
        Guid projectId,
        [FromQuery] Guid? versionId,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
            return BadRequest(Result<object>.Fail(["File is required."]));

        if (file.Length > UploadPipelineInputUseCase.MaxFileBytes)
            return BadRequest(Result<object>.Fail([$"File exceeds {UploadPipelineInputUseCase.MaxFileBytes / (1024 * 1024)} MB limit."]));

        if (!PipelineTextInputFileRules.IsAllowedExtension(file.FileName))
            return BadRequest(Result<object>.Fail(
                [$"Extension not allowed. Allowed types include {PipelineTextInputFileRules.AllowedHint}."]));

        await using var stream = file.OpenReadStream();
        var dto = await uploadUseCase.ExecuteAsync(
            projectId,
            versionId,
            stream,
            file.FileName ?? "upload",
            file.ContentType ?? "application/octet-stream",
            cancellationToken);
        if (dto == null)
            return NotFound();
        return Ok(Result<PipelineVersionFileItemDto>.Ok(dto));
    }

    [HttpPost("refine")]
    [ProducesResponseType(typeof(Result<RefinePipelineProjectResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Refine(
        Guid projectId,
        [FromBody] RefinePipelineProjectRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.ChangeSummary))
            return BadRequest(Result<object>.Fail(["changeSummary is required."]));

        var dto = await refineUseCase.ExecuteAsync(projectId, request, cancellationToken);
        if (dto == null)
            return NotFound();
        return Ok(Result<RefinePipelineProjectResponseDto>.Ok(dto));
    }
}
