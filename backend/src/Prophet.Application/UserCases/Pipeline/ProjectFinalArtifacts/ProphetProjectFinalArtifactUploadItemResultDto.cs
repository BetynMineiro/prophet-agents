namespace Prophet.Application.UserCases.Pipeline.ProjectFinalArtifacts;

public sealed record PipelineFinalArtifactUploadItemResultDto(
    string FileName,
    bool Success,
    string? ErrorMessage,
    PipelineFinalArtifactItemDto? Document);
