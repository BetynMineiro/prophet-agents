namespace Prophet.Application.UserCases.Pipeline.ProjectInputs;

public record PipelineInputUploadItemResultDto(
    string FileName,
    bool Success,
    string? ErrorMessage,
    PipelineInputDocumentItemDto? Document);
