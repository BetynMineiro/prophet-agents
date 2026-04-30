using Prophet.Application.Interfaces.Pipeline;
using Prophet.CrossCutting.RequestObjects;

namespace Prophet.Application.UserCases.Pipeline.Projects;

public class ListPipelineProjectsUseCase(IPipelineProjectStore store) : IListPipelineProjectsUseCase
{
    public async Task<IReadOnlyList<PipelineProjectItemDto>> ExecuteAsync(string? searchText, ActiveState activeState, CancellationToken cancellationToken = default)
    {
        var projects = await store.GetAllAsync(searchText, activeState, cancellationToken).ConfigureAwait(false);
        var ids = projects.Select(p => p.Id).ToList();
        var latestByProject = await store.GetLatestArtifactVersionPipelineByProjectIdsAsync(ids, cancellationToken).ConfigureAwait(false);
        return projects
            .Select(p => PipelineProjectItemDto.FromProject(
                p,
                latestByProject.TryGetValue(p.Id, out var tup) ? tup : null))
            .ToList();
    }
}
