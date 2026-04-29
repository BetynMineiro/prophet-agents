namespace Prophet.Application.UserCases.Pipeline.ProjectHtmlPocs;

public interface IDeletePipelineHtmlPocUseCase
{
    Task<bool> ExecuteAsync(Guid projectId, Guid documentId, CancellationToken cancellationToken = default);
}
