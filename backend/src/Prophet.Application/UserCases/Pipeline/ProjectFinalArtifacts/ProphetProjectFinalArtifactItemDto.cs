namespace Prophet.Application.UserCases.Pipeline.ProjectFinalArtifacts;

public sealed record PipelineFinalArtifactItemDto(
    Guid Id,
    string OriginalFileName,
    string ContentType,
    long SizeBytes,
    DateTime UploadedAtUtc);
