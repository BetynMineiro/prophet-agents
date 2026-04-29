namespace Prophet.Application.UserCases.Pipeline.Projects;

public interface IDeletePipelineProjectUseCase
{
    Task<bool> ExecuteAsync(Guid id, CancellationToken cancellationToken = default);
}
