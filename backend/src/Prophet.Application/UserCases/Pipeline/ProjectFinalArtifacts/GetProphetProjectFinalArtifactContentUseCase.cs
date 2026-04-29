using System.Text;
using Prophet.Application.Interfaces.Pipeline;
using Prophet.Application.Interfaces.Storage;
using Prophet.CrossCutting.Validation;

namespace Prophet.Application.UserCases.Pipeline.ProjectFinalArtifacts;

public sealed class GetPipelineFinalArtifactContentUseCase(
    IPipelineProjectStore projectStore,
    IPipelineFinalArtifactStore artifactStore,
    IStorageService storage,
    IValidator<PipelineFinalArtifactIdQuery> validator,
    IValidationErrorCollector errorCollector) : IGetPipelineFinalArtifactContentUseCase
{
    public async Task<PipelineFinalArtifactContentDto?> ExecuteAsync(
        Guid projectId,
        Guid documentId,
        CancellationToken cancellationToken = default)
    {
        var validation = validator.Validate(new PipelineFinalArtifactIdQuery(projectId, documentId));
        if (!validation.IsValid)
        {
            foreach (var error in validation.Errors)
                errorCollector.AddError(error);
            return null;
        }

        var project = await projectStore.GetByIdAsync(projectId, cancellationToken).ConfigureAwait(false);
        if (project == null)
            return null;

        var doc = await artifactStore.GetByIdAsync(projectId, documentId, cancellationToken).ConfigureAwait(false);
        if (doc == null)
            return null;

        var bytes = await storage.ReadObjectAsync(doc.StorageObjectPath, cancellationToken).ConfigureAwait(false);
        if (bytes == null)
            return null;

        if (bytes.Length == 0)
        {
            errorCollector.AddError("Artifact content is empty.");
            return null;
        }

        if (bytes.Length > FinalArtifactUploadLimits.MaxFileBytes)
        {
            errorCollector.AddError(
                $"Artifact exceeds {FinalArtifactUploadLimits.MaxFileBytes / (1024 * 1024)} MB limit.");
            return null;
        }

        var text = Encoding.UTF8.GetString(bytes);
        return new PipelineFinalArtifactContentDto(text);
    }
}
