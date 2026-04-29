using Prophet.Domain.Entities.Pipeline;

namespace Prophet.Application.Interfaces.Pipeline;

public interface IPipelineHtmlPocStore
{
    Task<IReadOnlyList<PipelineHtmlPoc>> ListByProjectAsync(Guid projectId, CancellationToken cancellationToken = default);

    Task<PipelineHtmlPoc?> GetByIdAsync(Guid projectId, Guid documentId, CancellationToken cancellationToken = default);

    /// <summary>Tracked entity for the given project and kind, if any.</summary>
    Task<PipelineHtmlPoc?> FindTrackedByProjectAndKindAsync(Guid projectId, HtmlPocKind kind, CancellationToken cancellationToken = default);

    Task<PipelineHtmlPoc> AddAsync(PipelineHtmlPoc entity, CancellationToken cancellationToken = default);

    /// <summary>Persist changes to tracked entities loaded from this context.</summary>
    Task SaveTrackedAsync(CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid projectId, Guid documentId, CancellationToken cancellationToken = default);
}
