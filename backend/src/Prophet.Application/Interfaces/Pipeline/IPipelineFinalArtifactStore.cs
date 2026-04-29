using Prophet.Domain.Entities.Pipeline;

namespace Prophet.Application.Interfaces.Pipeline;

public interface IPipelineFinalArtifactStore
{
    Task<IReadOnlyList<PipelineFinalArtifact>> ListByProjectAsync(Guid projectId, CancellationToken cancellationToken = default);

    Task<PipelineFinalArtifact?> GetByIdAsync(Guid projectId, Guid documentId, CancellationToken cancellationToken = default);

    Task<PipelineFinalArtifact> AddAsync(PipelineFinalArtifact entity, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid projectId, Guid documentId, CancellationToken cancellationToken = default);
}
