using Microsoft.EntityFrameworkCore;
using Prophet.Application.Interfaces.Pipeline;
using Prophet.Domain.Entities.Pipeline;

namespace Prophet.Adapters.Postgres.Pipeline;

public sealed class PipelineFinalArtifactStore(ProphetDbContext db) : IPipelineFinalArtifactStore
{
    public async Task<IReadOnlyList<PipelineFinalArtifact>> ListByProjectAsync(Guid projectId, CancellationToken cancellationToken = default) =>
        await db.PipelineFinalArtifacts.AsNoTracking()
            .Where(x => x.PipelineProjectId == projectId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

    public async Task<PipelineFinalArtifact?> GetByIdAsync(Guid projectId, Guid documentId, CancellationToken cancellationToken = default) =>
        await db.PipelineFinalArtifacts.AsNoTracking()
            .FirstOrDefaultAsync(x => x.PipelineProjectId == projectId && x.Id == documentId, cancellationToken);

    public async Task<PipelineFinalArtifact> AddAsync(PipelineFinalArtifact entity, CancellationToken cancellationToken = default)
    {
        db.PipelineFinalArtifacts.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<bool> DeleteAsync(Guid projectId, Guid documentId, CancellationToken cancellationToken = default)
    {
        var entity = await db.PipelineFinalArtifacts
            .FirstOrDefaultAsync(x => x.PipelineProjectId == projectId && x.Id == documentId, cancellationToken);
        if (entity == null)
            return false;
        db.PipelineFinalArtifacts.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
