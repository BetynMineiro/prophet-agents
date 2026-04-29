namespace Prophet.Application.UserCases.Pipeline.ProjectHtmlPocs;

public interface IListPipelineHtmlPocsUseCase
{
    Task<IReadOnlyList<PipelineHtmlPocItemDto>?> ExecuteAsync(Guid projectId, CancellationToken cancellationToken = default);
}
