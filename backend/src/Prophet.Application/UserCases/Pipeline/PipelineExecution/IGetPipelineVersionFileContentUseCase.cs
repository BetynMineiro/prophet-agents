namespace Prophet.Application.UserCases.Pipeline.PipelineExecution;

public interface IGetPipelineVersionFileContentUseCase
{
    Task<PipelineVersionFileContentDto?> ExecuteAsync(
        Guid projectId,
        Guid versionId,
        Guid fileId,
        CancellationToken cancellationToken = default);
}
