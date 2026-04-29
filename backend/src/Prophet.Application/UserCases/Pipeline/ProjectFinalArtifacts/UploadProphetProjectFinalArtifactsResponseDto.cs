namespace Prophet.Application.UserCases.Pipeline.ProjectFinalArtifacts;

public sealed record UploadPipelineFinalArtifactsResponseDto(
    IReadOnlyList<PipelineFinalArtifactUploadItemResultDto> Results);
