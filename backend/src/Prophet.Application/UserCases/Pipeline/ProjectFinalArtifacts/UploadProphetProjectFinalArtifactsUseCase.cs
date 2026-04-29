using Microsoft.Extensions.Options;
using Prophet.Application.Interfaces.Pipeline;
using Prophet.Application.Interfaces.Storage;
using Prophet.Application.Options;
using Prophet.Application.UserCases.Pipeline.ProjectInputs;
using Prophet.CrossCutting.Validation;
using Prophet.Domain.Entities.Pipeline;

namespace Prophet.Application.UserCases.Pipeline.ProjectFinalArtifacts;

public sealed class UploadPipelineFinalArtifactsUseCase(
    IPipelineProjectStore projectStore,
    IPipelineFinalArtifactStore artifactStore,
    IStorageService storage,
    IOptions<StorageOptions> storageOptions,
    IValidator<UploadPipelineFinalArtifactsRequest> validator,
    IValidationErrorCollector errorCollector) : IUploadPipelineFinalArtifactsUseCase
{
    private const string OwnerSegment = "prophet";
    private const string AssetType = "final-artifacts";

    public async Task<UploadPipelineFinalArtifactsResponseDto?> ExecuteAsync(
        Guid projectId,
        IReadOnlyList<InputFileChunk> files,
        CancellationToken cancellationToken = default)
    {
        var validation = validator.Validate(new UploadPipelineFinalArtifactsRequest(projectId, files));
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

        var results = new List<PipelineFinalArtifactUploadItemResultDto>();
        var productSegment = projectId.ToString("D");

        foreach (var file in files)
        {
            var item = await ProcessSingleFileAsync(
                projectId,
                userId,
                root,
                productSegment,
                file,
                cancellationToken).ConfigureAwait(false);
            results.Add(item);
        }

        return new UploadPipelineFinalArtifactsResponseDto(results);
    }

    private async Task<PipelineFinalArtifactUploadItemResultDto> ProcessSingleFileAsync(
        Guid projectId,
        Guid userId,
        string root,
        string productSegment,
        InputFileChunk file,
        CancellationToken cancellationToken)
    {
        var displayName = InputFileNameHelper.SanitizeOriginalFileName(file.OriginalFileName);
        if (!string.IsNullOrEmpty(file.SkipReason))
            return new PipelineFinalArtifactUploadItemResultDto(displayName, false, file.SkipReason, null);

        if (file.Content.Length == 0)
            return new PipelineFinalArtifactUploadItemResultDto(displayName, false, "File is empty.", null);

        if (!displayName.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
            return new PipelineFinalArtifactUploadItemResultDto(displayName, false, "Only .md files are allowed.", null);

        var contentType = NormalizeContentType(file.ContentType);
        var docId = Guid.NewGuid();
        var storageFileName = $"{docId:N}_{displayName}";

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

            var entity = new PipelineFinalArtifact
            {
                Id = docId,
                PipelineProjectId = projectId,
                OriginalFileName = displayName,
                ContentType = contentType,
                StorageObjectPath = objectPath,
                SizeBytes = file.Content.Length,
            };
            entity.SetCreatedBy(userId);
            var created = await artifactStore.AddAsync(entity, cancellationToken).ConfigureAwait(false);

            var dto = new PipelineFinalArtifactItemDto(
                created.Id,
                created.OriginalFileName,
                created.ContentType,
                created.SizeBytes,
                created.CreatedAtUtc);
            return new PipelineFinalArtifactUploadItemResultDto(displayName, true, null, dto);
        }
        catch (Exception ex)
        {
            return new PipelineFinalArtifactUploadItemResultDto(displayName, false, ex.Message, null);
        }
    }

    private static string NormalizeContentType(string? contentType)
    {
        var ct = string.IsNullOrWhiteSpace(contentType)
            ? "text/markdown"
            : contentType.Trim();
        if (!ct.Contains("markdown", StringComparison.OrdinalIgnoreCase) &&
            !ct.Contains("text/plain", StringComparison.OrdinalIgnoreCase) &&
            ct != "application/octet-stream")
        {
            return "text/markdown";
        }

        return ct;
    }
}
