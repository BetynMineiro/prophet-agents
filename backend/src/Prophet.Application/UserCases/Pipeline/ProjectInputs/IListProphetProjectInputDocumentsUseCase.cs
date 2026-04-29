namespace Prophet.Application.UserCases.Pipeline.ProjectInputs;

public interface IListPipelineInputDocumentsUseCase
{
    Task<IReadOnlyList<PipelineInputDocumentItemDto>?> ExecuteAsync(Guid projectId, CancellationToken cancellationToken = default);
}
