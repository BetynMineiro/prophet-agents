using System.Text.Json.Serialization;

namespace Prophet.Application.UserCases.Pipeline.PipelineExecution;

public record CreateArtifactVersionRequest(Guid? ParentVersionId, string? ChangeSummary);

/// <param name="InteractiveSteps">When true, runs one step then pauses until <c>continue</c> (or retry / rewind).</param>
public record RunPipelineRequest([property: JsonPropertyName("interactiveSteps")] bool InteractiveSteps = false);

public record RetryPipelineStepRequest([property: JsonPropertyName("stepIndex")] int StepIndex);

public record RewindPipelineToStepRequest([property: JsonPropertyName("targetStepIndex")] int TargetStepIndex);

public record ArtifactVersionItemDto(
    Guid Id,
    Guid PipelineProjectId,
    int VersionNumber,
    Guid? ParentVersionId,
    string? ChangeSummary,
    string PipelineStatus,
    int CurrentStepIndex,
    int TotalSteps,
    DateTime CreatedAtUtc);

public record PipelineStepStatusItemDto(string StepId, string Status);

public record PipelineRunStatusResponseDto(
    Guid VersionId,
    string PipelineStatus,
    int CurrentStepIndex,
    int TotalSteps,
    IReadOnlyList<PipelineStepStatusItemDto> Steps,
    string? Error,
    DateTime? StartedAtUtc,
    DateTime? CompletedAtUtc);

public record PipelineArtifactItemDto(string ArtifactType, string ContentJson, string? CreatedByAgent, DateTime CreatedAtUtc);

public record PipelineVersionFileItemDto(
    Guid Id,
    string FileType,
    string StorageObjectPath,
    string? OriginalFileName,
    string? DownloadUrl,
    DateTime CreatedAtUtc);

/// <summary>UTF-8 text body for pipeline version file preview (server-side read; avoids browser CORS on GCS signed URLs).</summary>
public sealed record PipelineVersionFileContentDto(string Text);

public record RefinePipelineProjectRequest(
    [property: JsonPropertyName("change_request")] string ChangeSummary);

public record RefinePipelineProjectResponseDto(Guid VersionId, int VersionNumber, int StartFromStepIndex);

public record RunPipelineResponseDto(Guid VersionId, string PipelineStatus, int CurrentStepIndex, int TotalSteps);
