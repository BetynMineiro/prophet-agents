using System.Collections.Concurrent;
using Prophet.Application.Interfaces.Pipeline;
using Prophet.CrossCutting.RequestObjects;
using Prophet.Domain.Entities.Pipeline;

namespace Prophet.Adapters.InMemory;

public class InMemoryPipelineProjectStore : IPipelineProjectStore
{
    private readonly ConcurrentDictionary<Guid, PipelineProject> _projects = new();
    private readonly ConcurrentDictionary<Guid, (Guid VersionId, PipelineRunStatus PipelineStatus)> _latestVersionByProject = new();

    public Task<PipelineProject?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _projects.TryGetValue(id, out var project);
        return Task.FromResult(project);
    }

    public Task<IReadOnlyList<PipelineProject>> GetAllAsync(
        string? searchText,
        ActiveState activeState,
        CancellationToken cancellationToken = default)
    {
        var query = _projects.Values.AsEnumerable();

        query = activeState switch
        {
            ActiveState.Active => query.Where(p => p.DeletedAtUtc == null),
            ActiveState.Inactive => query.Where(p => p.DeletedAtUtc != null),
            _ => query
        };

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var term = searchText.Trim();
            query = query.Where(p => p.Name.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        IReadOnlyList<PipelineProject> result = query.OrderByDescending(p => p.CreatedAtUtc).ToList();
        return Task.FromResult(result);
    }

    public Task<IReadOnlyDictionary<Guid, (Guid VersionId, PipelineRunStatus PipelineStatus)>> GetLatestArtifactVersionPipelineByProjectIdsAsync(
        IReadOnlyList<Guid> projectIds,
        CancellationToken cancellationToken = default)
    {
        var result = projectIds
            .Where(id => _latestVersionByProject.TryGetValue(id, out _))
            .ToDictionary(id => id, id => _latestVersionByProject[id]);

        return Task.FromResult<IReadOnlyDictionary<Guid, (Guid VersionId, PipelineRunStatus PipelineStatus)>>(result);
    }

    public Task<PipelineProject> CreateAsync(PipelineProject entity, CancellationToken cancellationToken = default)
    {
        _projects[entity.Id] = entity;
        return Task.FromResult(entity);
    }

    public Task<PipelineProject?> UpdateAsync(Guid id, string name, string? description, DateOnly? expectedDate, bool isActive, Guid updatedByUserId, CancellationToken cancellationToken = default)
    {
        if (!_projects.TryGetValue(id, out var project))
            return Task.FromResult<PipelineProject?>(null);

        project.Name = name;
        project.Description = description;
        project.ExpectedDate = expectedDate;
        if (!isActive && project.DeletedAtUtc == null)
            project.DeletedAtUtc = DateTime.UtcNow;
        else if (isActive)
            project.DeletedAtUtc = null;
        project.SetUpdatedBy(updatedByUserId);
        return Task.FromResult<PipelineProject?>(project);
    }

    public Task<PipelineProject?> RestoreAsync(Guid id, Guid restoredByUserId, CancellationToken cancellationToken = default)
    {
        if (!_projects.TryGetValue(id, out var project) || project.DeletedAtUtc == null)
            return Task.FromResult<PipelineProject?>(null);

        project.DeletedAtUtc = null;
        project.SetUpdatedBy(restoredByUserId);
        return Task.FromResult<PipelineProject?>(project);
    }

    public Task<bool> SoftDeleteAsync(Guid id, Guid deletedByUserId, CancellationToken cancellationToken = default)
    {
        if (!_projects.TryGetValue(id, out var project))
            return Task.FromResult(false);

        project.DeletedAtUtc = DateTime.UtcNow;
        project.SetUpdatedBy(deletedByUserId);
        return Task.FromResult(true);
    }

    /// <summary>Called by the execution store to keep pipeline status in sync for the project list.</summary>
    internal void UpdateLatestVersion(Guid projectId, Guid versionId, PipelineRunStatus status)
    {
        _latestVersionByProject[projectId] = (versionId, status);
    }
}
