namespace Prophet.Application.UserCases.Pipeline.Projects;

public interface ICreatePipelineProjectUseCase
{
    Task<PipelineProjectItemDto?> ExecuteAsync(CreatePipelineProjectRequest request, CancellationToken cancellationToken = default);
}
