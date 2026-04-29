namespace Prophet.Application.UserCases.Pipeline.PipelineExecution;

public interface IRefinePipelineProjectUseCase
{
    Task<RefinePipelineProjectResponseDto?> ExecuteAsync(Guid projectId, RefinePipelineProjectRequest request, CancellationToken cancellationToken = default);
}
