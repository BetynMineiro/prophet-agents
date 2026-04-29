namespace Prophet.Application.UserCases.Pipeline.Projects;

public interface IRestorePipelineProjectUseCase
{
    Task<PipelineProjectItemDto?> ExecuteAsync(Guid id, CancellationToken cancellationToken = default);
}
