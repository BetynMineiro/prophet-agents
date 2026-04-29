namespace Prophet.Application.UserCases.Pipeline.ProjectInputs;

public interface IDeletePipelineInputDocumentUseCase
{
    Task<bool> ExecuteAsync(Guid projectId, Guid documentId, CancellationToken cancellationToken = default);
}
