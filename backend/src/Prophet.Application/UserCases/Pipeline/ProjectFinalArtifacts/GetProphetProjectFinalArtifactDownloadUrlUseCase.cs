using Prophet.Application.Interfaces.Pipeline;
using Prophet.Application.Interfaces.Storage;
using Prophet.CrossCutting.Validation;

namespace Prophet.Application.UserCases.Pipeline.ProjectFinalArtifacts;

public sealed class GetPipelineFinalArtifactDownloadUrlUseCase(
    IPipelineProjectStore projectStore,
    IPipelineFinalArtifactStore artifactStore,
    IStorageService storage,
    IValidator<PipelineFinalArtifactIdQuery> validator,
    IValidationErrorCollector errorCollector) : IGetPipelineFinalArtifactDownloadUrlUseCase
{
    private static readonly TimeSpan SignedUrlDuration = TimeSpan.FromHours(1);

    public async Task<PipelineFinalArtifactDownloadDto?> ExecuteAsync(
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

        var url = await storage.GetSignedUrlAsync(
            doc.StorageObjectPath,
            SignedUrlDuration,
            doc.OriginalFileName,
            cancellationToken).ConfigureAwait(false);
        return new PipelineFinalArtifactDownloadDto(url ?? "");
    }
}
