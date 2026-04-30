using System.Collections.Concurrent;
using Prophet.Application.Interfaces.Pipeline;
using Prophet.Domain.Entities.Pipeline;

namespace Prophet.Adapters.InMemory;

public class InMemoryPipelineHtmlPocStore : IPipelineHtmlPocStore
{
    private readonly ConcurrentDictionary<Guid, PipelineHtmlPoc> _pocs = new();

    public Task<IReadOnlyList<PipelineHtmlPoc>> ListByProjectAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<PipelineHtmlPoc> result = _pocs.Values
            .Where(p => p.PipelineProjectId == projectId)
            .OrderBy(p => p.CreatedAtUtc)
            .ToList();
        return Task.FromResult(result);
    }

    public Task<PipelineHtmlPoc?> GetByIdAsync(Guid projectId, Guid documentId, CancellationToken cancellationToken = default)
    {
        _pocs.TryGetValue(documentId, out var poc);
        return Task.FromResult(poc?.PipelineProjectId == projectId ? poc : null);
    }

    public Task<PipelineHtmlPoc?> FindTrackedByProjectAndKindAsync(Guid projectId, HtmlPocKind kind, CancellationToken cancellationToken = default)
    {
        var poc = _pocs.Values.FirstOrDefault(p => p.PipelineProjectId == projectId && p.PocKind == kind);
        return Task.FromResult(poc);
    }

    public Task<PipelineHtmlPoc> AddAsync(PipelineHtmlPoc entity, CancellationToken cancellationToken = default)
    {
        _pocs[entity.Id] = entity;
        return Task.FromResult(entity);
    }

    public Task SaveTrackedAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task<bool> DeleteAsync(Guid projectId, Guid documentId, CancellationToken cancellationToken = default)
    {
        if (!_pocs.TryGetValue(documentId, out var poc) || poc.PipelineProjectId != projectId)
            return Task.FromResult(false);
        return Task.FromResult(_pocs.TryRemove(documentId, out _));
    }
}
