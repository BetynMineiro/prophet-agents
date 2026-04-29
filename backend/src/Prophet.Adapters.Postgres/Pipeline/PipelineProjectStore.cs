using Microsoft.EntityFrameworkCore;
using Prophet.Application.Interfaces.Pipeline;
using Prophet.CrossCutting.Extensions;
using Prophet.CrossCutting.RequestObjects;
using Prophet.CrossCutting.ResultObjects;
using Prophet.Domain.Entities.Pipeline;

namespace Prophet.Adapters.Postgres.Pipeline;

public sealed class PipelineProjectStore(ProphetDbContext db) : IPipelineProjectStore
{
    public async Task<IReadOnlyDictionary<Guid, (Guid VersionId, PipelineRunStatus PipelineStatus)>>
        GetLatestArtifactVersionPipelineByProjectIdsAsync(
            IReadOnlyList<Guid> projectIds,
            CancellationToken cancellationToken = default)
    {
        if (projectIds == null || projectIds.Count == 0)
            return new Dictionary<Guid, (Guid, PipelineRunStatus)>();

        var list = projectIds.Distinct().ToList();

        var maxPerProject = db.ArtifactVersions.AsNoTracking()
            .Where(v => list.Contains(v.PipelineProjectId))
            .GroupBy(v => v.PipelineProjectId)
            .Select(g => new { ProjectId = g.Key, MaxVersion = g.Max(v => v.VersionNumber) });

        var rows = await (
            from m in maxPerProject
            join v in db.ArtifactVersions.AsNoTracking() on new { P = m.ProjectId, N = m.MaxVersion }
                equals new { P = v.PipelineProjectId, N = v.VersionNumber }
            select new { v.PipelineProjectId, v.Id, v.PipelineStatus }).ToListAsync(cancellationToken);

        return rows.ToDictionary(r => r.PipelineProjectId, r => (r.Id, r.PipelineStatus));
    }

    public async Task<PipelineProject?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await db.PipelineProjects.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<CursorPage<PipelineProject>> GetPageAsync(
        int pageSize,
        string? cursor,
        string? searchText,
        ActiveState activeState,
        CancellationToken cancellationToken = default)
    {
        var cursorGuid = !string.IsNullOrWhiteSpace(cursor) && Guid.TryParse(cursor, out var parsed) ? parsed : (Guid?)null;
        var normalizedSearch = searchText.NormalizeSearchText(200);
        IQueryable<PipelineProject> query = db.PipelineProjects.AsNoTracking();
        query = activeState switch
        {
            ActiveState.Active => query.Where(p => p.DeletedAtUtc == null),
            ActiveState.Inactive => query.Where(p => p.DeletedAtUtc != null),
            _ => query
        };
        if (cursorGuid.HasValue)
            query = query.Where(p => p.Id.CompareTo(cursorGuid.Value) > 0);

#pragma warning disable CA1862
        query = query.Where(p => normalizedSearch == null
                                 || (p.Name != null && p.Name.ToLower().Contains(normalizedSearch))
                                 || (p.Description != null && p.Description.ToLower().Contains(normalizedSearch)));
#pragma warning restore CA1862

        query = query.OrderBy(p => p.Id);
        var items = await query.Take(pageSize + 1).ToListAsync(cancellationToken);
        var hasMore = items.Count > pageSize;
        if (hasMore)
            items = items.Take(pageSize).ToList();

        var nextCursor = hasMore && items.Count > 0 ? items[^1].Id.ToString() : null;
        return new CursorPage<PipelineProject> { Items = items, NextCursor = nextCursor, HasNext = hasMore };
    }

    public async Task<PipelineProject> CreateAsync(PipelineProject entity, CancellationToken cancellationToken = default)
    {
        db.PipelineProjects.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<PipelineProject?> UpdateAsync(Guid id, string name, string? description, DateOnly? expectedDate, bool isActive, Guid updatedByUserId, CancellationToken cancellationToken = default)
    {
        var entity = await db.PipelineProjects.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (entity == null)
            return null;
        entity.Name = name;
        entity.Description = description;
        entity.ExpectedDate = expectedDate;
        entity.DeletedAtUtc = isActive ? null : DateTime.UtcNow;
        entity.SetUpdatedBy(updatedByUserId);
        await db.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<PipelineProject?> RestoreAsync(Guid id, Guid restoredByUserId, CancellationToken cancellationToken = default)
    {
        var entity = await db.PipelineProjects.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (entity == null || entity.DeletedAtUtc == null)
            return null;
        entity.DeletedAtUtc = null;
        entity.SetUpdatedBy(restoredByUserId);
        await db.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<bool> SoftDeleteAsync(Guid id, Guid deletedByUserId, CancellationToken cancellationToken = default)
    {
        var entity = await db.PipelineProjects.FirstOrDefaultAsync(p => p.Id == id && p.DeletedAtUtc == null, cancellationToken);
        if (entity == null)
            return false;
        entity.DeletedAtUtc = DateTime.UtcNow;
        entity.SetUpdatedBy(deletedByUserId);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
