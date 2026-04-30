using System.Collections.Concurrent;
using Prophet.Application.Interfaces.Pipeline;
using Prophet.Domain.Entities.Pipeline;

namespace Prophet.Adapters.InMemory;

public class InMemoryPipelineFinalArtifactStore : IPipelineFinalArtifactStore
{
    private readonly ConcurrentDictionary<Guid, PipelineFinalArtifact> _artifacts = new();

    public Task<IReadOnlyList<PipelineFinalArtifact>> ListByProjectAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<PipelineFinalArtifact> result = _artifacts.Values
            .Where(a => a.PipelineProjectId == projectId)
            .OrderBy(a => a.CreatedAtUtc)
            .ToList();
        return Task.FromResult(result);
    }

    public Task<PipelineFinalArtifact?> GetByIdAsync(Guid projectId, Guid documentId, CancellationToken cancellationToken = default)
    {
        _artifacts.TryGetValue(documentId, out var artifact);
        return Task.FromResult(artifact?.PipelineProjectId == projectId ? artifact : null);
    }

    public Task<PipelineFinalArtifact> AddAsync(PipelineFinalArtifact entity, CancellationToken cancellationToken = default)
    {
        _artifacts[entity.Id] = entity;
        return Task.FromResult(entity);
    }

    public Task<bool> DeleteAsync(Guid projectId, Guid documentId, CancellationToken cancellationToken = default)
    {
        if (!_artifacts.TryGetValue(documentId, out var artifact) || artifact.PipelineProjectId != projectId)
            return Task.FromResult(false);
        return Task.FromResult(_artifacts.TryRemove(documentId, out _));
    }
}
