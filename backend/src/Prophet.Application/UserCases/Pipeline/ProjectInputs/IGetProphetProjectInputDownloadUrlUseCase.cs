namespace Prophet.Application.UserCases.Pipeline.ProjectInputs;

public interface IGetPipelineInputDownloadUrlUseCase
{
    Task<PipelineInputDownloadDto?> ExecuteAsync(Guid projectId, Guid documentId, CancellationToken cancellationToken = default);
}
