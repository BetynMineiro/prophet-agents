namespace Prophet.Application.UserCases.Pipeline.Projects;

public interface IGetPipelineProjectUseCase
{
    Task<PipelineProjectItemDto?> ExecuteAsync(Guid id, CancellationToken cancellationToken = default);
}
