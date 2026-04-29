using Microsoft.Extensions.Options;
using Prophet.Application.Interfaces.Pipeline;
using Prophet.Application.Interfaces.Storage;
using Prophet.Application.Options;
using Prophet.Application.UserCases.Pipeline.ProjectInputs;
using Prophet.Domain.Entities.Pipeline;

namespace Prophet.Application.UserCases.Pipeline.ProjectHtmlPocs;

public sealed class UploadPipelineHtmlPocUseCase(
    IPipelineProjectStore projectStore,
    IPipelineHtmlPocStore pocStore,
    IStorageService storage,
    IOptions<StorageOptions> storageOptions) : IUploadPipelineHtmlPocUseCase
{
    private const string OwnerSegment = "prophet";
    private const string AssetType = "html-pocs";

    public async Task<UploadPipelineHtmlPocExecutionResult?> ExecuteAsync(
        Guid projectId,
        HtmlPocKind kind,
        InputFileChunk file,
        bool replaceConfirmed,
        CancellationToken cancellationToken = default)
    {
        var project = await projectStore.GetByIdAsync(projectId, cancellationToken).ConfigureAwait(false);
        if (project == null)
            return null;

        var displayName = InputFileNameHelper.SanitizeOriginalFileName(file.OriginalFileName);
        var contentType = string.IsNullOrWhiteSpace(file.ContentType)
            ? "text/html"
            : file.ContentType.Trim();
        if (!contentType.Contains("html", StringComparison.OrdinalIgnoreCase) &&
            contentType != "application/octet-stream")
            contentType = "text/html";

        var existing = await pocStore.FindTrackedByProjectAndKindAsync(projectId, kind, cancellationToken).ConfigureAwait(false);
        if (existing != null && !replaceConfirmed)
            return new UploadPipelineHtmlPocExecutionResult(null, RequiresReplaceConfirmation: true);

        var userId = Guid.Empty;
        var root = storageOptions.Value.Root;
        if (string.IsNullOrWhiteSpace(root))
            root = "genesis";
        var productSegment = projectId.ToString("D");

        await using var stream = new MemoryStream(file.Content, writable: false);
        var docId = existing?.Id ?? Guid.NewGuid();
        var storageFileName = $"{docId:N}_{displayName}";
        var newObjectPath = await storage.UploadAsync(
            root,
            OwnerSegment,
            productSegment,
            AssetType,
            storageFileName,
            stream,
            contentType,
            cancellationToken).ConfigureAwait(false);

        if (existing != null)
        {
            var oldPath = existing.StorageObjectPath;
            existing.OriginalFileName = displayName;
            existing.ContentType = contentType;
            existing.StorageObjectPath = newObjectPath;
            existing.SizeBytes = file.Content.Length;
            existing.SetUpdatedBy(userId);
            await pocStore.SaveTrackedAsync(cancellationToken).ConfigureAwait(false);
            if (!string.IsNullOrWhiteSpace(oldPath) && !string.Equals(oldPath, newObjectPath, StringComparison.Ordinal))
                await storage.DeleteObjectAsync(oldPath, cancellationToken).ConfigureAwait(false);

            var dto = new PipelineHtmlPocItemDto(
                existing.Id,
                existing.PocKind,
                existing.OriginalFileName,
                existing.ContentType,
                existing.SizeBytes,
                existing.UpdatedAtUtc ?? existing.CreatedAtUtc);
            return new UploadPipelineHtmlPocExecutionResult(dto, false);
        }

        var entity = new PipelineHtmlPoc
        {
            Id = docId,
            PipelineProjectId = projectId,
            PocKind = kind,
            OriginalFileName = displayName,
            ContentType = contentType,
            StorageObjectPath = newObjectPath,
            SizeBytes = file.Content.Length,
        };
        entity.SetCreatedBy(userId);
        var created = await pocStore.AddAsync(entity, cancellationToken).ConfigureAwait(false);
        var createdDto = new PipelineHtmlPocItemDto(
            created.Id,
            created.PocKind,
            created.OriginalFileName,
            created.ContentType,
            created.SizeBytes,
            created.CreatedAtUtc);
        return new UploadPipelineHtmlPocExecutionResult(createdDto, false);
    }
}
