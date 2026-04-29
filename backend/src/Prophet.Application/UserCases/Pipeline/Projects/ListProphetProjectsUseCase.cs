using Prophet.Application.Interfaces.Pipeline;
using Prophet.CrossCutting.RequestObjects;
using Prophet.CrossCutting.ResultObjects;

namespace Prophet.Application.UserCases.Pipeline.Projects;

public class ListPipelineProjectsUseCase(IPipelineProjectStore store) : IListPipelineProjectsUseCase
{
    public async Task<CursorPage<PipelineProjectItemDto>> ExecuteAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        var pageSize = Math.Clamp(request.PageSize, 1, 500);
        var page = await store.GetPageAsync(pageSize, request.Cursor, request.SearchText, request.ActiveState, cancellationToken).ConfigureAwait(false);
        var ids = page.Items.Select(p => p.Id).ToList();
        var latestByProject = await store.GetLatestArtifactVersionPipelineByProjectIdsAsync(ids, cancellationToken).ConfigureAwait(false);
        var items = page.Items
            .Select(p => PipelineProjectItemDto.FromProject(
                p,
                latestByProject.TryGetValue(p.Id, out var tup) ? tup : null))
            .ToList();
        return new CursorPage<PipelineProjectItemDto> { Items = items, NextCursor = page.NextCursor, HasNext = page.HasNext };
    }
}
