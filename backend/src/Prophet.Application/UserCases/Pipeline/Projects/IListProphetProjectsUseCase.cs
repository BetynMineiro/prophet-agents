using Prophet.CrossCutting.RequestObjects;

namespace Prophet.Application.UserCases.Pipeline.Projects;

public interface IListPipelineProjectsUseCase
{
    Task<IReadOnlyList<PipelineProjectItemDto>> ExecuteAsync(string? searchText, ActiveState activeState, CancellationToken cancellationToken = default);
}
