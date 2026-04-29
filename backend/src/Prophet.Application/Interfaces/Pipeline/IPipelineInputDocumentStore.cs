using Prophet.Domain.Entities.Pipeline;

namespace Prophet.Application.Interfaces.Pipeline;

public interface IPipelineInputDocumentStore
{
    Task<IReadOnlyList<PipelineInputDocument>> ListByProjectAsync(Guid projectId, CancellationToken cancellationToken = default);

    Task<PipelineInputDocument?> GetByIdAsync(Guid projectId, Guid documentId, CancellationToken cancellationToken = default);

    Task<PipelineInputDocument> AddAsync(PipelineInputDocument entity, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid projectId, Guid documentId, CancellationToken cancellationToken = default);
}
