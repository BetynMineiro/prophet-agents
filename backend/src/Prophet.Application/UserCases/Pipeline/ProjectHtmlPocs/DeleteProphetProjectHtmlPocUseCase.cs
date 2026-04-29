using Prophet.Application.Interfaces.Pipeline;
using Prophet.Application.Interfaces.Storage;
using Prophet.CrossCutting.Validation;

namespace Prophet.Application.UserCases.Pipeline.ProjectHtmlPocs;

public sealed class DeletePipelineHtmlPocUseCase(
    IPipelineProjectStore projectStore,
    IPipelineHtmlPocStore pocStore,
    IStorageService storage,
    IValidator<PipelineHtmlPocIdQuery> validator,
    IValidationErrorCollector errorCollector) : IDeletePipelineHtmlPocUseCase
{
    public async Task<bool> ExecuteAsync(Guid projectId, Guid documentId, CancellationToken cancellationToken = default)
    {
        var validation = validator.Validate(new PipelineHtmlPocIdQuery(projectId, documentId));
        if (!validation.IsValid)
        {
            foreach (var error in validation.Errors)
                errorCollector.AddError(error);
            return false;
        }

        var project = await projectStore.GetByIdAsync(projectId, cancellationToken).ConfigureAwait(false);
        if (project == null)
            return false;

        var doc = await pocStore.GetByIdAsync(projectId, documentId, cancellationToken).ConfigureAwait(false);
        if (doc == null)
            return false;

        await storage.DeleteObjectAsync(doc.StorageObjectPath, cancellationToken).ConfigureAwait(false);
        return await pocStore.DeleteAsync(projectId, documentId, cancellationToken).ConfigureAwait(false);
    }
}
