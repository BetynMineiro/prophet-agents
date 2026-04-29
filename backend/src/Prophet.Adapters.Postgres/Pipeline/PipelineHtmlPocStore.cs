using Microsoft.EntityFrameworkCore;
using Prophet.Application.Interfaces.Pipeline;
using Prophet.Domain.Entities.Pipeline;

namespace Prophet.Adapters.Postgres.Pipeline;

public sealed class PipelineHtmlPocStore(ProphetDbContext db) : IPipelineHtmlPocStore
{
    public async Task<IReadOnlyList<PipelineHtmlPoc>> ListByProjectAsync(Guid projectId, CancellationToken cancellationToken = default) =>
        await db.PipelineHtmlPocs.AsNoTracking()
            .Where(x => x.PipelineProjectId == projectId)
            .OrderBy(x => x.PocKind)
            .ToListAsync(cancellationToken);

    public async Task<PipelineHtmlPoc?> GetByIdAsync(Guid projectId, Guid documentId, CancellationToken cancellationToken = default) =>
        await db.PipelineHtmlPocs.AsNoTracking()
            .FirstOrDefaultAsync(x => x.PipelineProjectId == projectId && x.Id == documentId, cancellationToken);

    public async Task<PipelineHtmlPoc?> FindTrackedByProjectAndKindAsync(Guid projectId, HtmlPocKind kind, CancellationToken cancellationToken = default) =>
        await db.PipelineHtmlPocs
            .FirstOrDefaultAsync(x => x.PipelineProjectId == projectId && x.PocKind == kind, cancellationToken);

    public async Task<PipelineHtmlPoc> AddAsync(PipelineHtmlPoc entity, CancellationToken cancellationToken = default)
    {
        db.PipelineHtmlPocs.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        return entity;
    }

    /// <summary>Persists changes to an entity already tracked (e.g. from <see cref="FindTrackedByProjectAndKindAsync"/>).</summary>
    public async Task SaveTrackedAsync(CancellationToken cancellationToken = default) =>
        await db.SaveChangesAsync(cancellationToken);

    public async Task<bool> DeleteAsync(Guid projectId, Guid documentId, CancellationToken cancellationToken = default)
    {
        var entity = await db.PipelineHtmlPocs
            .FirstOrDefaultAsync(x => x.PipelineProjectId == projectId && x.Id == documentId, cancellationToken);
        if (entity == null)
            return false;
        db.PipelineHtmlPocs.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
