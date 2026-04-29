namespace Prophet.Application.UserCases.Pipeline.Projects;

public interface IUpdatePipelineProjectUseCase
{
    Task<PipelineProjectItemDto?> ExecuteAsync(Guid id, UpdatePipelineProjectRequest request, CancellationToken cancellationToken = default);
}
