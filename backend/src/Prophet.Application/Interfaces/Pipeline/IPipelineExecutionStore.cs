using Prophet.CrossCutting.ResultObjects;
using Prophet.Domain.Entities.Pipeline;

namespace Prophet.Application.Interfaces.Pipeline;

/// <summary>Persistence for MAF artifact versions, JSON artifacts, and version-scoped files (§5).</summary>
public interface IPipelineExecutionStore
{
    Task<bool> ProjectExistsActiveAsync(Guid projectId, CancellationToken cancellationToken = default);

    Task<ArtifactVersion?> GetVersionAsync(Guid versionId, CancellationToken cancellationToken = default);

    Task<ArtifactVersion?> GetVersionForProjectAsync(Guid projectId, Guid versionId, CancellationToken cancellationToken = default);

    /// <summary>Tracked entity for pipeline state updates (same request scope).</summary>
    Task<ArtifactVersion?> GetVersionForUpdateAsync(Guid projectId, Guid versionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// If the row has all steps done but is still <see cref="PipelineRunStatus.Running"/>, set Completed in the database.
    /// Fixes rare cases where <see cref="ArtifactVersion.CurrentStepIndex"/> persisted but terminal status did not.
    /// </summary>
    Task<bool> ReconcileRunningToCompletedWhenAllStepsDoneAsync(
        Guid projectId,
        Guid versionId,
        CancellationToken cancellationToken = default);

    Task PersistChangesAsync(CancellationToken cancellationToken = default);

    Task<int> GetMaxVersionNumberAsync(Guid projectId, CancellationToken cancellationToken = default);

    Task<ArtifactVersion> AddVersionAsync(ArtifactVersion entity, CancellationToken cancellationToken = default);

    Task UpdateVersionAsync(ArtifactVersion entity, CancellationToken cancellationToken = default);

    Task<CursorPage<ArtifactVersion>> ListVersionsPageAsync(
        Guid projectId,
        int pageSize,
        string? cursorVersionNumber,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PipelineArtifact>> ListArtifactsAsync(Guid versionId, CancellationToken cancellationToken = default);

    Task<PipelineArtifact?> GetArtifactByTypeAsync(Guid versionId, string artifactType, CancellationToken cancellationToken = default);

    Task<PipelineArtifact> AddArtifactAsync(PipelineArtifact entity, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PipelineVersionFile>> ListFilesAsync(Guid versionId, CancellationToken cancellationToken = default);

    Task<PipelineVersionFile?> GetFileAsync(Guid versionId, Guid fileId, CancellationToken cancellationToken = default);

    Task<PipelineVersionFile> AddFileAsync(PipelineVersionFile entity, CancellationToken cancellationToken = default);

    /// <summary>Removes pipeline artifact rows for this version and types (interactive rewind / retry).</summary>
    Task<int> DeleteArtifactsAsync(Guid versionId, IReadOnlyCollection<string> artifactTypes, CancellationToken cancellationToken = default);

    /// <summary>Removes specific version file rows by id (after storage delete).</summary>
    Task<int> DeleteFilesByIdsAsync(Guid versionId, IReadOnlyCollection<Guid> fileIds, CancellationToken cancellationToken = default);
}
