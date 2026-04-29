using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Prophet.Application.UserCases.Pipeline.PipelineExecution;
using Prophet.CrossCutting.ResultObjects;

namespace Prophet.Api.Controllers.Pipeline;

/// <summary>Pipeline run, status, JSON artifacts, and version files (§7).</summary>
[ApiController]
[Route("v1/prophet/projects/{projectId:guid}/versions/{versionId:guid}")]
[Produces("application/json")]
[EnableRateLimiting("api")]
public class ProjectVersionPipelineController(
    IRunPipelineUseCase runUseCase,
    IContinuePipelineStepUseCase continuePipelineStepUseCase,
    IRetryPipelineStepUseCase retryPipelineStepUseCase,
    IRewindPipelineToStepUseCase rewindPipelineToStepUseCase,
    IGetPipelineStatusUseCase statusUseCase,
    IListPipelineArtifactsUseCase listArtifactsUseCase,
    IGetPipelineArtifactByTypeUseCase getArtifactUseCase,
    IListPipelineVersionFilesUseCase listFilesUseCase,
    IGetPipelineVersionFileContentUseCase getFileContentUseCase) : ControllerBase
{
    [HttpPost("run")]
    [ProducesResponseType(typeof(Result<RunPipelineResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Run(
        Guid projectId,
        Guid versionId,
        [FromBody] RunPipelineRequest? request,
        CancellationToken cancellationToken)
    {
        var outcome = await runUseCase.ExecuteAsync(projectId, versionId, request, cancellationToken);
        return MapRunOutcome(outcome);
    }

    [HttpPost("pipeline/continue")]
    [ProducesResponseType(typeof(Result<RunPipelineResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ContinuePipeline(Guid projectId, Guid versionId, CancellationToken cancellationToken)
    {
        var outcome = await continuePipelineStepUseCase.ExecuteAsync(projectId, versionId, cancellationToken);
        return MapRunOutcome(outcome);
    }

    [HttpPost("pipeline/steps/retry")]
    [ProducesResponseType(typeof(Result<RunPipelineResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RetryStep(
        Guid projectId,
        Guid versionId,
        [FromBody] RetryPipelineStepRequest request,
        CancellationToken cancellationToken)
    {
        var outcome = await retryPipelineStepUseCase.ExecuteAsync(projectId, versionId, request, cancellationToken);
        return MapRunOutcome(outcome);
    }

    [HttpPost("pipeline/steps/rewind")]
    [ProducesResponseType(typeof(Result<RunPipelineResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RewindToStep(
        Guid projectId,
        Guid versionId,
        [FromBody] RewindPipelineToStepRequest request,
        CancellationToken cancellationToken)
    {
        var outcome = await rewindPipelineToStepUseCase.ExecuteAsync(projectId, versionId, request, cancellationToken);
        return MapRunOutcome(outcome);
    }

    [HttpGet("status")]
    [ProducesResponseType(typeof(Result<PipelineRunStatusResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<PipelineRunStatusResponseDto>>> Status(
        Guid projectId,
        Guid versionId,
        CancellationToken cancellationToken)
    {
        var dto = await statusUseCase.ExecuteAsync(projectId, versionId, cancellationToken);
        if (dto == null)
            return NotFound();
        return Ok(Result<PipelineRunStatusResponseDto>.Ok(dto));
    }

    [HttpGet("artifacts")]
    [ProducesResponseType(typeof(Result<IReadOnlyList<PipelineArtifactItemDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<IReadOnlyList<PipelineArtifactItemDto>>>> ListArtifacts(
        Guid projectId,
        Guid versionId,
        CancellationToken cancellationToken)
    {
        var list = await listArtifactsUseCase.ExecuteAsync(projectId, versionId, cancellationToken);
        if (list == null)
            return NotFound();
        return Ok(Result<IReadOnlyList<PipelineArtifactItemDto>>.Ok(list));
    }

    [HttpGet("artifacts/{artifactType}")]
    [ProducesResponseType(typeof(Result<PipelineArtifactItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<PipelineArtifactItemDto>>> GetArtifact(
        Guid projectId,
        Guid versionId,
        string artifactType,
        CancellationToken cancellationToken)
    {
        var dto = await getArtifactUseCase.ExecuteAsync(projectId, versionId, artifactType, cancellationToken);
        if (dto == null)
            return NotFound();
        return Ok(Result<PipelineArtifactItemDto>.Ok(dto));
    }

    [HttpGet("files")]
    [ProducesResponseType(typeof(Result<IReadOnlyList<PipelineVersionFileItemDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<IReadOnlyList<PipelineVersionFileItemDto>>>> ListFiles(
        Guid projectId,
        Guid versionId,
        CancellationToken cancellationToken)
    {
        var list = await listFilesUseCase.ExecuteAsync(projectId, versionId, cancellationToken);
        if (list == null)
            return NotFound();
        return Ok(Result<IReadOnlyList<PipelineVersionFileItemDto>>.Ok(list));
    }

    /// <summary>File body for markdown/text preview. Server reads GCS — browser fetch to signed URLs hits CORS.</summary>
    [HttpGet("files/{fileId:guid}/content")]
    [ProducesResponseType(typeof(Result<PipelineVersionFileContentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<PipelineVersionFileContentDto>>> GetFileContent(
        Guid projectId,
        Guid versionId,
        Guid fileId,
        CancellationToken cancellationToken)
    {
        var dto = await getFileContentUseCase.ExecuteAsync(projectId, versionId, fileId, cancellationToken).ConfigureAwait(false);
        if (dto == null)
            return NotFound();
        return Ok(Result<PipelineVersionFileContentDto>.Ok(dto));
    }

    private IActionResult MapRunOutcome(RunPipelineOutcome outcome)
    {
        return outcome.Kind switch
        {
            RunPipelineOutcomeKind.Ok => Ok(Result<RunPipelineResponseDto>.Ok(outcome.Data!)),
            RunPipelineOutcomeKind.ProjectOrVersionNotFound => NotFound(),
            RunPipelineOutcomeKind.ConflictAlreadyRunningOrCompleted => Conflict(
                Result<object>.Fail(["Pipeline is already running for this version."])),
            RunPipelineOutcomeKind.ConflictInvalidPipelineState => Conflict(
                Result<object>.Fail(
                    [
                        "Pipeline state does not allow this action (e.g. use Continue when paused; Run restarts from step 1 when Idle, Failed, or Completed).",
                    ])),
            _ => NotFound()
        };
    }
}
