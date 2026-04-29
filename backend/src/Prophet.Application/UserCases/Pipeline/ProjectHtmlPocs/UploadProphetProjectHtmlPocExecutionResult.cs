namespace Prophet.Application.UserCases.Pipeline.ProjectHtmlPocs;

public sealed record UploadPipelineHtmlPocExecutionResult(
    PipelineHtmlPocItemDto? Document,
    bool RequiresReplaceConfirmation);
