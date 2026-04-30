using System.Collections.Concurrent;
using Prophet.Application.Interfaces.Pipeline;
using Prophet.Domain.Entities.Pipeline;

namespace Prophet.Adapters.InMemory;

public class InMemoryPipelineInputDocumentStore : IPipelineInputDocumentStore
{
    private readonly ConcurrentDictionary<Guid, PipelineInputDocument> _docs = new();

    public Task<IReadOnlyList<PipelineInputDocument>> ListByProjectAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<PipelineInputDocument> result = _docs.Values
            .Where(d => d.PipelineProjectId == projectId)
            .OrderBy(d => d.CreatedAtUtc)
            .ToList();
        return Task.FromResult(result);
    }

    public Task<PipelineInputDocument?> GetByIdAsync(Guid projectId, Guid documentId, CancellationToken cancellationToken = default)
    {
        _docs.TryGetValue(documentId, out var doc);
        return Task.FromResult(doc?.PipelineProjectId == projectId ? doc : null);
    }

    public Task<PipelineInputDocument> AddAsync(PipelineInputDocument entity, CancellationToken cancellationToken = default)
    {
        _docs[entity.Id] = entity;
        return Task.FromResult(entity);
    }

    public Task<bool> DeleteAsync(Guid projectId, Guid documentId, CancellationToken cancellationToken = default)
    {
        if (!_docs.TryGetValue(documentId, out var doc) || doc.PipelineProjectId != projectId)
            return Task.FromResult(false);
        return Task.FromResult(_docs.TryRemove(documentId, out _));
    }
}
