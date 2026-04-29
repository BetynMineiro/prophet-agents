using Microsoft.EntityFrameworkCore;
using Prophet.Application.Interfaces.Pipeline;
using Prophet.Domain.Entities.Pipeline;

namespace Prophet.Adapters.Postgres.Pipeline;

public sealed class PipelineInputDocumentStore(ProphetDbContext db) : IPipelineInputDocumentStore
{
    public async Task<IReadOnlyList<PipelineInputDocument>> ListByProjectAsync(Guid projectId, CancellationToken cancellationToken = default) =>
        await db.PipelineInputDocuments.AsNoTracking()
            .Where(x => x.PipelineProjectId == projectId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

    public async Task<PipelineInputDocument?> GetByIdAsync(Guid projectId, Guid documentId, CancellationToken cancellationToken = default) =>
        await db.PipelineInputDocuments.AsNoTracking()
            .FirstOrDefaultAsync(x => x.PipelineProjectId == projectId && x.Id == documentId, cancellationToken);

    public async Task<PipelineInputDocument> AddAsync(PipelineInputDocument entity, CancellationToken cancellationToken = default)
    {
        db.PipelineInputDocuments.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<bool> DeleteAsync(Guid projectId, Guid documentId, CancellationToken cancellationToken = default)
    {
        var entity = await db.PipelineInputDocuments
            .FirstOrDefaultAsync(x => x.PipelineProjectId == projectId && x.Id == documentId, cancellationToken);
        if (entity == null)
            return false;
        db.PipelineInputDocuments.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
