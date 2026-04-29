using Prophet.CrossCutting.RequestObjects;
using Prophet.CrossCutting.ResultObjects;

namespace Prophet.Application.UserCases.Pipeline.PipelineExecution;

public interface IListArtifactVersionsUseCase
{
    Task<CursorPage<ArtifactVersionItemDto>?> ExecuteAsync(Guid projectId, PagedRequest request, CancellationToken cancellationToken = default);
}
