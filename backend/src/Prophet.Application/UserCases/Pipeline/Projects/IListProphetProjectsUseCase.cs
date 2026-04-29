using Prophet.CrossCutting.RequestObjects;
using Prophet.CrossCutting.ResultObjects;

namespace Prophet.Application.UserCases.Pipeline.Projects;

public interface IListPipelineProjectsUseCase
{
    Task<CursorPage<PipelineProjectItemDto>> ExecuteAsync(PagedRequest request, CancellationToken cancellationToken = default);
}
