namespace Prophet.Application.UserCases.Pipeline.ProjectInputs;

public record UploadPipelineInputDocumentsResponseDto(
    IReadOnlyList<PipelineInputUploadItemResultDto> Results);
