using Prophet.Application.Interfaces.Pipeline;
using Prophet.CrossCutting.RequestObjects;
using Prophet.CrossCutting.ResultObjects;

namespace Prophet.Application.UserCases.Pipeline.PipelineExecution;

public sealed class ListArtifactVersionsUseCase(
    IPipelineExecutionStore pipelineStore) : IListArtifactVersionsUseCase
{
    public async Task<CursorPage<ArtifactVersionItemDto>?> ExecuteAsync(
        Guid projectId,
        PagedRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!await pipelineStore.ProjectExistsActiveAsync(projectId, cancellationToken).ConfigureAwait(false))
            return null;

        var page = await pipelineStore.ListVersionsPageAsync(
            projectId,
            request.PageSize,
            request.Cursor,
            cancellationToken).ConfigureAwait(false);
        var dtos = page.Items.Select(PipelineExecutionStatusHelper.ToItemDto).ToList();
        return new CursorPage<ArtifactVersionItemDto>
        {
            Items = dtos,
            NextCursor = page.NextCursor,
            HasNext = page.HasNext
        };
    }
}
