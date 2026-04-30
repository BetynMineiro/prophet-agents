using System.Collections.Concurrent;
using Prophet.Application.Interfaces.Pipeline;
using Prophet.CrossCutting.ResultObjects;
using Prophet.Domain.Entities.Pipeline;

namespace Prophet.Adapters.InMemory;

public class InMemoryPipelineExecutionStore(InMemoryPipelineProjectStore projectStore) : IPipelineExecutionStore
{
    private readonly ConcurrentDictionary<Guid, ArtifactVersion> _versions = new();
    private readonly ConcurrentDictionary<Guid, PipelineArtifact> _artifacts = new();
    private readonly ConcurrentDictionary<Guid, PipelineVersionFile> _files = new();

    public Task<bool> ProjectExistsActiveAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        var exists = projectStore.GetByIdAsync(projectId, cancellationToken).Result is { DeletedAtUtc: null };
        return Task.FromResult(exists);
    }

    public Task<ArtifactVersion?> GetVersionAsync(Guid versionId, CancellationToken cancellationToken = default)
    {
        _versions.TryGetValue(versionId, out var v);
        return Task.FromResult(v);
    }

    public Task<ArtifactVersion?> GetVersionForProjectAsync(Guid projectId, Guid versionId, CancellationToken cancellationToken = default)
    {
        _versions.TryGetValue(versionId, out var v);
        return Task.FromResult(v?.PipelineProjectId == projectId ? v : null);
    }

    public Task<ArtifactVersion?> GetVersionForUpdateAsync(Guid projectId, Guid versionId, CancellationToken cancellationToken = default) =>
        GetVersionForProjectAsync(projectId, versionId, cancellationToken);

    public Task<bool> ReconcileRunningToCompletedWhenAllStepsDoneAsync(Guid projectId, Guid versionId, CancellationToken cancellationToken = default)
    {
        if (!_versions.TryGetValue(versionId, out var v) || v.PipelineProjectId != projectId)
            return Task.FromResult(false);

        if (v.PipelineStatus != PipelineRunStatus.Running)
            return Task.FromResult(false);

        var totalSteps = MainPipelineStepIds.StepIds.Count;
        if (v.CurrentStepIndex < totalSteps)
            return Task.FromResult(false);

        v.PipelineStatus = PipelineRunStatus.Completed;
        v.PipelineCompletedAtUtc = DateTime.UtcNow;
        SyncLatestVersion(v);
        return Task.FromResult(true);
    }

    public Task PersistChangesAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task<int> GetMaxVersionNumberAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        var max = _versions.Values
            .Where(v => v.PipelineProjectId == projectId)
            .Select(v => v.VersionNumber)
            .DefaultIfEmpty(0)
            .Max();
        return Task.FromResult(max);
    }

    public Task<ArtifactVersion> AddVersionAsync(ArtifactVersion entity, CancellationToken cancellationToken = default)
    {
        _versions[entity.Id] = entity;
        SyncLatestVersion(entity);
        return Task.FromResult(entity);
    }

    public Task UpdateVersionAsync(ArtifactVersion entity, CancellationToken cancellationToken = default)
    {
        _versions[entity.Id] = entity;
        SyncLatestVersion(entity);
        return Task.CompletedTask;
    }

    public Task<CursorPage<ArtifactVersion>> ListVersionsPageAsync(Guid projectId, int pageSize, string? cursorVersionNumber, CancellationToken cancellationToken = default)
    {
        var all = _versions.Values
            .Where(v => v.PipelineProjectId == projectId)
            .OrderByDescending(v => v.VersionNumber)
            .ToList();

        if (!string.IsNullOrWhiteSpace(cursorVersionNumber) && int.TryParse(cursorVersionNumber, out var cursorNum))
            all = all.Where(v => v.VersionNumber < cursorNum).ToList();

        var items = all.Take(pageSize).ToList();
        var hasNext = all.Count > pageSize;
        var nextCursor = hasNext ? items.Last().VersionNumber.ToString() : null;

        return Task.FromResult(new CursorPage<ArtifactVersion> { Items = items, HasNext = hasNext, NextCursor = nextCursor });
    }

    public Task<IReadOnlyList<PipelineArtifact>> ListArtifactsAsync(Guid versionId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<PipelineArtifact> result = _artifacts.Values
            .Where(a => a.VersionId == versionId)
            .OrderBy(a => a.CreatedAtUtc)
            .ToList();
        return Task.FromResult(result);
    }

    public Task<PipelineArtifact?> GetArtifactByTypeAsync(Guid versionId, string artifactType, CancellationToken cancellationToken = default)
    {
        var artifact = _artifacts.Values
            .FirstOrDefault(a => a.VersionId == versionId && a.ArtifactType == artifactType);
        return Task.FromResult(artifact);
    }

    public Task<PipelineArtifact> AddArtifactAsync(PipelineArtifact entity, CancellationToken cancellationToken = default)
    {
        _artifacts[entity.Id] = entity;
        return Task.FromResult(entity);
    }

    public Task<IReadOnlyList<PipelineVersionFile>> ListFilesAsync(Guid versionId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<PipelineVersionFile> result = _files.Values
            .Where(f => f.VersionId == versionId)
            .OrderBy(f => f.CreatedAtUtc)
            .ToList();
        return Task.FromResult(result);
    }

    public Task<PipelineVersionFile?> GetFileAsync(Guid versionId, Guid fileId, CancellationToken cancellationToken = default)
    {
        _files.TryGetValue(fileId, out var file);
        return Task.FromResult(file?.VersionId == versionId ? file : null);
    }

    public Task<PipelineVersionFile> AddFileAsync(PipelineVersionFile entity, CancellationToken cancellationToken = default)
    {
        _files[entity.Id] = entity;
        return Task.FromResult(entity);
    }

    public Task<int> DeleteArtifactsAsync(Guid versionId, IReadOnlyCollection<string> artifactTypes, CancellationToken cancellationToken = default)
    {
        var toRemove = _artifacts.Values
            .Where(a => a.VersionId == versionId && artifactTypes.Contains(a.ArtifactType))
            .Select(a => a.Id)
            .ToList();
        foreach (var id in toRemove)
            _artifacts.TryRemove(id, out _);
        return Task.FromResult(toRemove.Count);
    }

    public Task<int> DeleteFilesByIdsAsync(Guid versionId, IReadOnlyCollection<Guid> fileIds, CancellationToken cancellationToken = default)
    {
        var removed = 0;
        foreach (var id in fileIds)
        {
            if (_files.TryGetValue(id, out var f) && f.VersionId == versionId && _files.TryRemove(id, out _))
                removed++;
        }
        return Task.FromResult(removed);
    }

    private void SyncLatestVersion(ArtifactVersion v)
    {
        var currentMax = _versions.Values
            .Where(x => x.PipelineProjectId == v.PipelineProjectId)
            .Select(x => x.VersionNumber)
            .DefaultIfEmpty(0)
            .Max();
        if (v.VersionNumber >= currentMax)
            projectStore.UpdateLatestVersion(v.PipelineProjectId, v.Id, v.PipelineStatus);
    }
}
