using Microsoft.Extensions.Options;
using Prophet.Application.Interfaces.Pipeline;
using Prophet.Application.Interfaces.Storage;
using Prophet.Application.Options;
using Prophet.CrossCutting.Validation;
using Prophet.Domain.Entities.Pipeline;

namespace Prophet.Application.UserCases.Pipeline.ProjectInputs;

public sealed class UploadPipelineInputDocumentsUseCase(
    IPipelineProjectStore projectStore,
    IPipelineInputDocumentStore documentStore,
    IStorageService storage,
    IOptions<StorageOptions> storageOptions,
    IValidator<UploadPipelineInputDocumentsRequest> validator,
    IValidationErrorCollector errorCollector) : IUploadPipelineInputDocumentsUseCase
{
    /// <summary>Larger than legacy 2 MB to allow typical PDF/DOC inputs; still bounded for API abuse.</summary>
    public const long MaxFileBytes = 10 * 1024 * 1024;
    private const string OwnerSegment = "prophet";
    private const string AssetType = "inputs";

    public async Task<UploadPipelineInputDocumentsResponseDto?> ExecuteAsync(
        Guid projectId,
        IReadOnlyList<InputFileChunk> files,
        CancellationToken cancellationToken = default)
    {
        var validation = validator.Validate(new UploadPipelineInputDocumentsRequest(projectId, files));
        if (!validation.IsValid)
        {
            foreach (var error in validation.Errors)
                errorCollector.AddError(error);
            return null;
        }

        var project = await projectStore.GetByIdAsync(projectId, cancellationToken).ConfigureAwait(false);
        if (project == null)
            return null;

        var userId = Guid.Empty;
        var root = storageOptions.Value.Root;
        if (string.IsNullOrWhiteSpace(root))
            root = "genesis";

        var results = new List<PipelineInputUploadItemResultDto>();
        var productSegment = projectId.ToString("D");

        foreach (var file in files)
        {
            var displayName = InputFileNameHelper.SanitizeOriginalFileName(file.OriginalFileName);
            if (!string.IsNullOrEmpty(file.SkipReason))
            {
                results.Add(new PipelineInputUploadItemResultDto(displayName, false, file.SkipReason, null));
                continue;
            }

            if (file.Content.Length == 0)
            {
                results.Add(new PipelineInputUploadItemResultDto(displayName, false, "File is empty.", null));
                continue;
            }

            var docId = Guid.NewGuid();
            var storageFileName = $"{docId:N}_{displayName}";
            var contentType = string.IsNullOrWhiteSpace(file.ContentType)
                ? "application/octet-stream"
                : file.ContentType.Trim();

            try
            {
                await using var stream = new MemoryStream(file.Content, writable: false);
                var objectPath = await storage.UploadAsync(
                    root,
                    OwnerSegment,
                    productSegment,
                    AssetType,
                    storageFileName,
                    stream,
                    contentType,
                    cancellationToken).ConfigureAwait(false);

                var entity = new PipelineInputDocument
                {
                    Id = docId,
                    PipelineProjectId = projectId,
                    OriginalFileName = displayName,
                    ContentType = contentType,
                    StorageObjectPath = objectPath,
                    SizeBytes = file.Content.Length,
                };
                entity.SetCreatedBy(userId);
                var created = await documentStore.AddAsync(entity, cancellationToken).ConfigureAwait(false);

                var dto = new PipelineInputDocumentItemDto(
                    created.Id,
                    created.OriginalFileName,
                    created.ContentType,
                    created.SizeBytes,
                    created.CreatedAtUtc);
                results.Add(new PipelineInputUploadItemResultDto(displayName, true, null, dto));
            }
            catch (Exception ex)
            {
                results.Add(new PipelineInputUploadItemResultDto(displayName, false, ex.Message, null));
            }
        }

        return new UploadPipelineInputDocumentsResponseDto(results);
    }
}
