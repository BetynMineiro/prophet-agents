namespace Prophet.Application.UserCases.Pipeline.PipelineExecution;

public interface IListPipelineVersionFilesUseCase
{
    Task<IReadOnlyList<PipelineVersionFileItemDto>?> ExecuteAsync(Guid projectId, Guid versionId, CancellationToken cancellationToken = default);
}
