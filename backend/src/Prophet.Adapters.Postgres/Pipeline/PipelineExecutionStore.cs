using Microsoft.EntityFrameworkCore;
using Prophet.Application.Interfaces.Pipeline;
using Prophet.CrossCutting.ResultObjects;
using Prophet.Domain.Entities.Pipeline;

namespace Prophet.Adapters.Postgres.Pipeline;

public sealed class PipelineExecutionStore(ProphetDbContext db) : IPipelineExecutionStore
{
    private static readonly int TotalPipelineSteps = MainPipelineStepIds.TotalSteps;

    public async Task<bool> ProjectExistsActiveAsync(Guid projectId, CancellationToken cancellationToken = default) =>
        await db.PipelineProjects.AsNoTracking()
            .AnyAsync(p => p.Id == projectId && p.DeletedAtUtc == null, cancellationToken);

    public async Task<ArtifactVersion?> GetVersionAsync(Guid versionId, CancellationToken cancellationToken = default) =>
        await db.ArtifactVersions.AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == versionId, cancellationToken);

    public async Task<ArtifactVersion?> GetVersionForProjectAsync(Guid projectId, Guid versionId, CancellationToken cancellationToken = default) =>
        await db.ArtifactVersions.AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == versionId && v.PipelineProjectId == projectId, cancellationToken);

    public async Task<ArtifactVersion?> GetVersionForUpdateAsync(Guid projectId, Guid versionId, CancellationToken cancellationToken = default) =>
        await db.ArtifactVersions
            .FirstOrDefaultAsync(v => v.Id == versionId && v.PipelineProjectId == projectId, cancellationToken);

    public async Task<bool> ReconcileRunningToCompletedWhenAllStepsDoneAsync(
        Guid projectId,
        Guid versionId,
        CancellationToken cancellationToken = default)
    {
        var utc = DateTime.UtcNow;
        var affected = await db.ArtifactVersions
            .Where(x =>
                x.Id == versionId
                && x.PipelineProjectId == projectId
                && x.CurrentStepIndex >= TotalPipelineSteps
                && x.PipelineStatus == PipelineRunStatus.Running)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(p => p.PipelineStatus, PipelineRunStatus.Completed)
                    .SetProperty(p => p.PipelineCompletedAtUtc, utc),
                cancellationToken)
            .ConfigureAwait(false);
        return affected > 0;
    }

    public Task PersistChangesAsync(CancellationToken cancellationToken = default) =>
        db.SaveChangesAsync(cancellationToken);

    public async Task<int> GetMaxVersionNumberAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        var max = await db.ArtifactVersions.AsNoTracking()
            .Where(v => v.PipelineProjectId == projectId)
            .Select(v => (int?)v.VersionNumber)
            .MaxAsync(cancellationToken);
        return max ?? 0;
    }

    public async Task<ArtifactVersion> AddVersionAsync(ArtifactVersion entity, CancellationToken cancellationToken = default)
    {
        db.ArtifactVersions.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task UpdateVersionAsync(ArtifactVersion entity, CancellationToken cancellationToken = default)
    {
        db.ArtifactVersions.Update(entity);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<CursorPage<ArtifactVersion>> ListVersionsPageAsync(
        Guid projectId,
        int pageSize,
        string? cursorVersionNumber,
        CancellationToken cancellationToken = default)
    {
        int? cursor = null;
        if (!string.IsNullOrWhiteSpace(cursorVersionNumber)
            && int.TryParse(cursorVersionNumber.Trim(), out var parsed)
            && parsed >= 1)
            cursor = parsed;

        var query = db.ArtifactVersions.AsNoTracking()
            .Where(v => v.PipelineProjectId == projectId);
        if (cursor.HasValue)
            query = query.Where(v => v.VersionNumber < cursor.Value);

        query = query.OrderByDescending(v => v.VersionNumber);
        var items = await query.Take(pageSize + 1).ToListAsync(cancellationToken);
        var hasMore = items.Count > pageSize;
        if (hasMore)
            items = items.Take(pageSize).ToList();

        var nextCursor = hasMore && items.Count > 0 ? items[^1].VersionNumber.ToString() : null;
        return new CursorPage<ArtifactVersion> { Items = items, NextCursor = nextCursor, HasNext = hasMore };
    }

    public async Task<IReadOnlyList<PipelineArtifact>> ListArtifactsAsync(Guid versionId, CancellationToken cancellationToken = default) =>
        await db.PipelineArtifacts.AsNoTracking()
            .Where(a => a.VersionId == versionId)
            .OrderBy(a => a.ArtifactType)
            .ToListAsync(cancellationToken);

    public async Task<PipelineArtifact?> GetArtifactByTypeAsync(Guid versionId, string artifactType, CancellationToken cancellationToken = default) =>
        await db.PipelineArtifacts.AsNoTracking()
            .FirstOrDefaultAsync(a => a.VersionId == versionId && a.ArtifactType == artifactType, cancellationToken);

    public async Task<PipelineArtifact> AddArtifactAsync(PipelineArtifact entity, CancellationToken cancellationToken = default)
    {
        var existing = await db.PipelineArtifacts
            .FirstOrDefaultAsync(
                a => a.VersionId == entity.VersionId && a.ArtifactType == entity.ArtifactType,
                cancellationToken)
            .ConfigureAwait(false);
        if (existing != null)
        {
            existing.ContentJson = entity.ContentJson;
            existing.CreatedByAgent = entity.CreatedByAgent;
            existing.CreatedAtUtc = entity.CreatedAtUtc;
            await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return existing;
        }

        db.PipelineArtifacts.Add(entity);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return entity;
    }

    public async Task<IReadOnlyList<PipelineVersionFile>> ListFilesAsync(Guid versionId, CancellationToken cancellationToken = default) =>
        await db.PipelineVersionFiles.AsNoTracking()
            .Where(f => f.VersionId == versionId)
            .OrderBy(f => f.FileType)
            .ThenBy(f => f.CreatedAtUtc)
            .ToListAsync(cancellationToken);

    public async Task<PipelineVersionFile?> GetFileAsync(Guid versionId, Guid fileId, CancellationToken cancellationToken = default) =>
        await db.PipelineVersionFiles.AsNoTracking()
            .FirstOrDefaultAsync(f => f.VersionId == versionId && f.Id == fileId, cancellationToken);

    public async Task<PipelineVersionFile> AddFileAsync(PipelineVersionFile entity, CancellationToken cancellationToken = default)
    {
        db.PipelineVersionFiles.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<int> DeleteArtifactsAsync(Guid versionId, IReadOnlyCollection<string> artifactTypes, CancellationToken cancellationToken = default)
    {
        if (artifactTypes.Count == 0)
            return 0;

        var rows = await db.PipelineArtifacts
            .Where(a => a.VersionId == versionId && artifactTypes.Contains(a.ArtifactType))
            .ToListAsync(cancellationToken);
        db.PipelineArtifacts.RemoveRange(rows);
        await db.SaveChangesAsync(cancellationToken);
        return rows.Count;
    }

    public async Task<int> DeleteFilesByIdsAsync(Guid versionId, IReadOnlyCollection<Guid> fileIds, CancellationToken cancellationToken = default)
    {
        if (fileIds.Count == 0)
            return 0;

        var rows = await db.PipelineVersionFiles
            .Where(f => f.VersionId == versionId && fileIds.Contains(f.Id))
            .ToListAsync(cancellationToken);
        db.PipelineVersionFiles.RemoveRange(rows);
        await db.SaveChangesAsync(cancellationToken);
        return rows.Count;
    }
}
