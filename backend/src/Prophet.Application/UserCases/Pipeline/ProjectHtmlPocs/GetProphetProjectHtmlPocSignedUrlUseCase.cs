using Prophet.Application.Interfaces.Pipeline;
using Prophet.Application.Interfaces.Storage;
using Prophet.CrossCutting.Validation;

namespace Prophet.Application.UserCases.Pipeline.ProjectHtmlPocs;

public sealed class GetPipelineHtmlPocSignedUrlUseCase(
    IPipelineProjectStore projectStore,
    IPipelineHtmlPocStore pocStore,
    IStorageService storage,
    IValidator<PipelineHtmlPocIdQuery> validator,
    IValidationErrorCollector errorCollector) : IGetPipelineHtmlPocSignedUrlUseCase
{
    private static readonly TimeSpan SignedUrlDuration = TimeSpan.FromHours(1);

    public async Task<PipelineHtmlPocDownloadDto?> ExecuteAsync(
        Guid projectId,
        Guid documentId,
        bool asAttachment,
        CancellationToken cancellationToken = default)
    {
        var validation = validator.Validate(new PipelineHtmlPocIdQuery(projectId, documentId));
        if (!validation.IsValid)
        {
            foreach (var error in validation.Errors)
                errorCollector.AddError(error);
            return null;
        }

        var project = await projectStore.GetByIdAsync(projectId, cancellationToken).ConfigureAwait(false);
        if (project == null)
            return null;

        var doc = await pocStore.GetByIdAsync(projectId, documentId, cancellationToken).ConfigureAwait(false);
        if (doc == null)
            return null;

        var url = await storage.GetSignedUrlAsync(
            doc.StorageObjectPath,
            SignedUrlDuration,
            asAttachment ? doc.OriginalFileName : null,
            cancellationToken).ConfigureAwait(false);
        return new PipelineHtmlPocDownloadDto(url ?? string.Empty);
    }
}
