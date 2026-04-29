namespace Prophet.Application.UserCases.Pipeline.ProjectHtmlPocs;

public interface IGetPipelineHtmlPocContentUseCase
{
    Task<PipelineHtmlPocContentDto?> ExecuteAsync(
        Guid projectId,
        Guid documentId,
        CancellationToken cancellationToken = default);
}
