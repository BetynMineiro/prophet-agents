using Prophet.Application.Interfaces.Pipeline;
using Prophet.Application.Interfaces.Storage;
using Prophet.CrossCutting.Validation;

namespace Prophet.Application.UserCases.Pipeline.ProjectInputs;

public sealed class DeletePipelineInputDocumentUseCase(
    IPipelineProjectStore projectStore,
    IPipelineInputDocumentStore documentStore,
    IStorageService storage,
    IValidator<PipelineInputDocumentIdQuery> validator,
    IValidationErrorCollector errorCollector) : IDeletePipelineInputDocumentUseCase
{
    public async Task<bool> ExecuteAsync(Guid projectId, Guid documentId, CancellationToken cancellationToken = default)
    {
        var validation = validator.Validate(new PipelineInputDocumentIdQuery(projectId, documentId));
        if (!validation.IsValid)
        {
            foreach (var error in validation.Errors)
                errorCollector.AddError(error);
            return false;
        }

        var project = await projectStore.GetByIdAsync(projectId, cancellationToken).ConfigureAwait(false);
        if (project == null)
            return false;

        var doc = await documentStore.GetByIdAsync(projectId, documentId, cancellationToken).ConfigureAwait(false);
        if (doc == null)
            return false;

        await storage.DeleteObjectAsync(doc.StorageObjectPath, cancellationToken).ConfigureAwait(false);
        return await documentStore.DeleteAsync(projectId, documentId, cancellationToken).ConfigureAwait(false);
    }
}
