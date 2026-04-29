namespace Prophet.Application.UserCases.Pipeline.ProjectHtmlPocs;

public interface IGetPipelineHtmlPocSignedUrlUseCase
{
    /// <param name="asAttachment">When true, URL forces download; when false, suitable for opening in a new tab.</param>
    Task<PipelineHtmlPocDownloadDto?> ExecuteAsync(
        Guid projectId,
        Guid documentId,
        bool asAttachment,
        CancellationToken cancellationToken = default);
}
