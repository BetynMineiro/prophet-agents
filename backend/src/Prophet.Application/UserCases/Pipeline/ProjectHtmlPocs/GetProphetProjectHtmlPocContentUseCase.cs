using System.Text;
using Prophet.Application.Interfaces.Pipeline;
using Prophet.Application.Interfaces.Storage;
using Prophet.CrossCutting.Validation;

namespace Prophet.Application.UserCases.Pipeline.ProjectHtmlPocs;

public sealed class GetPipelineHtmlPocContentUseCase(
    IPipelineProjectStore projectStore,
    IPipelineHtmlPocStore pocStore,
    IStorageService storage,
    IValidator<PipelineHtmlPocIdQuery> validator,
    IValidationErrorCollector errorCollector) : IGetPipelineHtmlPocContentUseCase
{
    public async Task<PipelineHtmlPocContentDto?> ExecuteAsync(
        Guid projectId,
        Guid documentId,
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

        var bytes = await storage.ReadObjectAsync(doc.StorageObjectPath, cancellationToken).ConfigureAwait(false);
        if (bytes == null)
            return null;

        if (bytes.Length == 0)
        {
            errorCollector.AddError("HTML POC content is empty.");
            return null;
        }

        if (bytes.Length > HtmlPocUploadLimits.MaxFileBytes)
        {
            errorCollector.AddError(
                $"HTML POC exceeds {HtmlPocUploadLimits.MaxFileBytes / (1024 * 1024)} MB limit.");
            return null;
        }

        var text = Encoding.UTF8.GetString(bytes);
        return new PipelineHtmlPocContentDto(text);
    }
}
