namespace Prophet.Application.UserCases.Pipeline.ProjectInputs;

public record PipelineInputDocumentItemDto(
    Guid Id,
    string OriginalFileName,
    string ContentType,
    long SizeBytes,
    DateTime UploadedAtUtc);
