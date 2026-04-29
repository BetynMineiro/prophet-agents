namespace Prophet.Application.UserCases.Pipeline.PipelineExecution;

public interface IGetPipelineStatusUseCase
{
    Task<PipelineRunStatusResponseDto?> ExecuteAsync(Guid projectId, Guid versionId, CancellationToken cancellationToken = default);
}
