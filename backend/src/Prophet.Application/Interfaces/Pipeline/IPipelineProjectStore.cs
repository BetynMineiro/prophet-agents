using Prophet.CrossCutting.RequestObjects;
using Prophet.CrossCutting.ResultObjects;
using Prophet.Domain.Entities.Pipeline;

namespace Prophet.Application.Interfaces.Pipeline;

public interface IPipelineProjectStore
{
    /// <summary>By id only (includes soft-deleted rows for admin edit/restore).</summary>
    Task<PipelineProject?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<CursorPage<PipelineProject>> GetPageAsync(
        int pageSize,
        string? cursor,
        string? searchText,
        ActiveState activeState,
        CancellationToken cancellationToken = default);

    /// <summary>Latest artifact version per project (by <see cref="ArtifactVersion.VersionNumber"/>), for list/detail pipeline summary.</summary>
    Task<IReadOnlyDictionary<Guid, (Guid VersionId, PipelineRunStatus PipelineStatus)>> GetLatestArtifactVersionPipelineByProjectIdsAsync(
        IReadOnlyList<Guid> projectIds,
        CancellationToken cancellationToken = default);

    Task<PipelineProject> CreateAsync(PipelineProject entity, CancellationToken cancellationToken = default);

    /// <summary>Updates name, description and expected date. If soft-deleted, clears <c>DeletedAtUtc</c> (reactivate). Returns null if not found.</summary>
    Task<PipelineProject?> UpdateAsync(Guid id, string name, string? description, DateOnly? expectedDate, bool isActive, Guid updatedByUserId, CancellationToken cancellationToken = default);

    /// <summary>Clears <see cref="Prophet.Domain.Entities.Pipeline.PipelineProject.DeletedAtUtc"/> when the project was soft-deleted; returns null if missing or already active.</summary>
    Task<PipelineProject?> RestoreAsync(Guid id, Guid restoredByUserId, CancellationToken cancellationToken = default);

    /// <summary>Sets <see cref="Prophet.Domain.Entities.Pipeline.PipelineProject.DeletedAtUtc"/> and audit fields (same pattern as update).</summary>
    Task<bool> SoftDeleteAsync(Guid id, Guid deletedByUserId, CancellationToken cancellationToken = default);
}
